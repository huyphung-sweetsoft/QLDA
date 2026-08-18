using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Utils
{
    public static class DataTableExtensions
    {
        public static bool IsAnonymous(Type type)
        {
            if (type.IsGenericType)
            {
                var d = type.GetGenericTypeDefinition();
                if (d.IsClass && d.IsSealed && d.Attributes.HasFlag(TypeAttributes.NotPublic))
                {
                    var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        //WOW! We have an anonymous type!!!
                        return true;
                    }
                }
            }
            return false;
        }

        public static DataTable ToDataTable<T>(this IList<T> data) where T : new()
        {
            DataTable table = new DataTable();
            if (data == null || data.Count == 0)
                return table;

            //special handling for value types and string
            if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)) || data[0].GetType() == typeof(string))
            {
                DataColumn dc = new DataColumn("Value");
                table.Columns.Add(dc);
                foreach (T item in data)
                {
                    DataRow dr = table.NewRow();
                    dr[0] = item;
                    table.Rows.Add(dr);
                }
            }
            else
            {
                if (IsAnonymous(data[0].GetType()))
                {
                    PropertyInfo[] properties = data[0].GetType().GetProperties();
                    if (properties != null && properties.Length > 0)
                    {
                        foreach (PropertyInfo prop in properties)
                            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        foreach (T item in data)
                        {
                            DataRow row = table.NewRow();
                            foreach (PropertyInfo prop in properties)
                                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                            table.Rows.Add(row);
                        }
                    }
                }
                else
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                    if (properties != null && properties.Count > 0)
                    {
                        foreach (PropertyDescriptor prop in properties)
                            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        foreach (T item in data)
                        {
                            DataRow row = table.NewRow();
                            foreach (PropertyDescriptor prop in properties)
                                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                            table.Rows.Add(row);
                        }
                    }
                }
            }
            return table;
        }

        public static T ToEmement<T>(this DataTable dt)
        {
            List<T> lst = dt.ToList<T>();
            if (lst.Count > 0) return lst[0];
            return default(T);
        }
        public static List<T> ToList<T>(this DataSet ds)
        {
            List<T> lst = new System.Collections.Generic.List<T>();
            if (ds != null && ds.Tables.Count > 0)
            {
                lst = ds.Tables[0].ToList<T>();
            }
            return lst;
        }
        public static List<T> ToList<T>(this DataTable dt)
        {
            List<T> lst = new System.Collections.Generic.List<T>();
            Type tClass = typeof(T);
            PropertyInfo[] pClass = tClass.GetProperties();
            List<DataColumn> dc = dt.Columns.Cast<DataColumn>().ToList();
            T cn;
            foreach (DataRow item in dt.Rows)
            {
                cn = (T)Activator.CreateInstance(tClass);
                foreach (PropertyInfo pc in pClass)
                {
                    try
                    {

                        DataColumn d = dc.Find(c => (c.ColumnName.ToLower().Replace("_", string.Empty) == pc.Name.ToLower()));
                        if (d != null)
                        {
                            object value = item[d.ColumnName];
                            if (value == null) continue;
                            switch (d.DataType.FullName)
                            {
                                case "System.UInt16":
                                case "System.Int16":
                                    Int16 value16 = -1;
                                    if (Int16.TryParse(value.ToString(), out value16))
                                        pc.SetValue(cn, value16, null);
                                    break;
                                case "System.UInt32":
                                case "System.Int32":
                                    Int32 value32 = -1;
                                    if (Int32.TryParse(value.ToString(), out value32)) ;
                                    pc.SetValue(cn, value32, null);
                                    break;
                                case "System.UInt64":
                                case "System.Int64":
                                    Int64 value64 = -1;
                                    if (Int64.TryParse(value.ToString(), out value64))
                                        pc.SetValue(cn, value64, null);
                                    break;
                                case "System.Decimal":
                                case "System.Double":
                                case "System.Single":
                                    Decimal decima = -1;
                                    if (Decimal.TryParse(value.ToString(), out decima))
                                        pc.SetValue(cn, decima, null);
                                    break;
                                case "System.Boolean":
                                    bool b = false;
                                    Boolean.TryParse(value.ToString(), out b);
                                    pc.SetValue(cn, b, null);
                                    break;
                                case "System.DateTime":
                                    DateTime date = DateTime.MinValue;
                                    DateTime.TryParse(value.ToString(), out date);
                                    pc.SetValue(cn, date, null);
                                    break;
                                case "System.Byte[]":
                                    BinaryFormatter bf = new BinaryFormatter();
                                    byte[] arr = value as byte[];
                                    string vl = Encoding.UTF8.GetString(arr);
                                    pc.SetValue(cn, vl, null);
                                    break;
                                case "System.Guid":
                                    Guid g = Guid.Empty;
                                    Guid.TryParse(value.ToString(), out g);
                                    pc.SetValue(cn, g, null);
                                    break;
                                default:
                                    if (pc.PropertyType == typeof(Boolean))
                                        pc.SetValue(cn, bool.Parse(value.ToString()), null);
                                    else if (value.GetType() != typeof(DBNull))
                                        pc.SetValue(cn, value, null);
                                    break;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                lst.Add(cn);
            }
            return lst;
        }
        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            var propertiesNameList = properties.Where(x => row.Table.Columns.Contains(x.Name)).ToList();
            foreach (var property in propertiesNameList)
            {
                try
                {
                    property.SetValue(item, row[property.Name], null);
                }
                catch
                {
                }
            }
            return item;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties, Dictionary<string, string> mappings) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (mappings.ContainsKey(property.Name))
                    property.SetValue(item, row[mappings[property.Name]], null);
            }
            return item;
        }

        public static DataTable ConvertRowToDataTable(DataRow row)
        {
            DataTable newTable = row.Table.Clone(); // Copy cấu trúc (column) từ bảng gốc
            newTable.ImportRow(row);                // Copy dữ liệu từ dòng
            return newTable;
        }

        #region DataTable, DataSet

        public static List<T> ToCollection<T>(this DataTable dt)
        {
            List<T> lst = new System.Collections.Generic.List<T>();
            Type tClass = typeof(T);
            PropertyInfo[] pClass = tClass.GetProperties();
            List<DataColumn> dc = dt.Columns.Cast<DataColumn>().ToList();
            T cn;
            foreach (DataRow item in dt.Rows)
            {
                cn = (T)Activator.CreateInstance(tClass);
                foreach (PropertyInfo pc in pClass)
                {
                    try
                    {

                        DataColumn d = dc.Find(c => (c.ColumnName.ToLower().Replace("_", string.Empty) == pc.Name.ToLower()));
                        if (d != null)
                        {
                            string value = item[d.ColumnName].ToString();
                            switch (d.DataType.FullName)
                            {
                                case "System.Byte":
                                    byte valueByte = byte.Parse(value);
                                    pc.SetValue(cn, valueByte, null);
                                    break;
                                case "System.UInt16":
                                case "System.Int16":
                                    Int16 value16 = Int16.Parse(value);
                                    pc.SetValue(cn, value16, null);
                                    break;
                                case "System.UInt32":
                                case "System.Int32":
                                    Int32 value32 = Int32.Parse(value);
                                    pc.SetValue(cn, value32, null);
                                    break;
                                case "System.UInt64":
                                case "System.Int64":
                                    Int64 value64 = Int64.Parse(value);
                                    pc.SetValue(cn, value64, null);
                                    break;
                                case "System.Decimal":
                                case "System.Double":
                                    Decimal valued = Decimal.Parse(value);
                                    pc.SetValue(cn, valued, null);
                                    break;
                                case "System.Boolean":
                                    bool b = false;
                                    Boolean.TryParse(value, out b);
                                    pc.SetValue(cn, b, null);
                                    break;
                                case "System.Guid":
                                    Guid g = Guid.Empty;
                                    Guid.TryParse(value, out g);
                                    pc.SetValue(cn, g, null);
                                    break;
                                case "System.DateTime":
                                    DateTime date = DateTime.MinValue;
                                    DateTime.TryParse(value, out date);
                                    pc.SetValue(cn, date, null);
                                    break;
                                default:
                                    pc.SetValue(cn, value, null);
                                    break;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                lst.Add(cn);
            }
            return lst;
        }

        public static List<T> ToCollection<T>(this DataSet ds)
        {
            List<T> lst = new System.Collections.Generic.List<T>();
            if (ds != null && ds.Tables.Count > 0)
            {
                lst = ds.Tables[0].ToCollection<T>();
            }
            return lst;
        }
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            DataTable table = new DataTable();
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            if (data != null && data.Count > 0)
            {
                foreach (PropertyDescriptor prop in properties)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
            }
            return table;
        }
        #endregion
    }
}
