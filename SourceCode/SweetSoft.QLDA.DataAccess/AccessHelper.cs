using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.DataAccess
{
    public class AccessHelper
    {
        public static void UpdateHelper(Type columType, Type tableType, object newItem, object inItem
            , SubSonic.TableSchema.TableColumnCollection columns, SubSonic.TableSchema.TableColumnCollection dirtyColumns)
        {
            PropertyInfo propertyInfo;
            int indexColumn = 0;
            foreach (FieldInfo fieldInfo in columType.GetFields())
            {
                //if (exceptColumns.FindIndex(e => e == fieldInfo.Name) != -1)
                //    continue;
                propertyInfo = tableType.GetProperty(fieldInfo.Name);
                var itemValue = propertyInfo.GetValue(newItem, null);
                var hValue = propertyInfo.GetValue(inItem, null);
                propertyInfo.SetValue(newItem, hValue, null);
                if ((hValue == null && itemValue == null)
                    || (hValue != null && itemValue != null && (hValue.ToString() == itemValue.ToString())))
                    dirtyColumns = HandlerAddDirtyColumns(columns[indexColumn], dirtyColumns);
                indexColumn++;
            }
        }
        private static SubSonic.TableSchema.TableColumnCollection HandlerAddDirtyColumns(SubSonic.TableSchema.TableColumn column, SubSonic.TableSchema.TableColumnCollection dirtyColumns)
        {
            if (dirtyColumns.GetColumn(column.ColumnName) == null)
                dirtyColumns.Add(column);
            return dirtyColumns;
        }
    }
}
