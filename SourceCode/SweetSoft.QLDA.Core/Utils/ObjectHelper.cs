using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Utils
{
    // Attribute để đánh dấu thuộc tính không copy
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreCopyAttribute : Attribute
    {
    }
    public static class ObjectHelper
    {
        // Danh sách các thuộc tính mặc định của SubSonic cần bỏ qua
        private static readonly HashSet<string> SubSonicDefaultProperties = new HashSet<string>
    {
        "IsLoaded",
        "IsNew",
        "IsDirty",
        "IsValid",
        "Errors",
        "Columns",
        "TableName",
        "Provider",
        "Schema",
        "Descriptor",
        "BinaryImports",
        "HasManyLists",
        "PrimaryKey",
        "KeyID",
        "ColumnName",
        "TableColumns",
        "ProviderName",
        "DataContext",
        "ValidateWhenSaving",
        "DirtyColumns",
        "NullExceptionMessage",
        "InvalidTypeExceptionMessage",
        "LengthExceptionMessage",
        "ApplicationId"
    };

        public static void CopyChangedProperties<T>(T source, T destination)
        {
            CopyChangedProperties(source, destination, null, null);
        }

        public static void CopyChangedProperties<T>(T source, T destination, params string[] excludeProperties)
        {
            CopyChangedProperties(source, destination, excludeProperties?.ToHashSet(), null, true);
        }

        public static void CopyChangedProperties<T>(T source, T destination, params Expression<Func<T, object>>[] excludeExpressions)
        {
            var excludeProps = excludeExpressions?.Select(GetPropertyName).ToHashSet();
            CopyChangedProperties(source, destination, excludeProps, null, true);
        }

        public static void CopySpecificProperties<T>(T source, T destination, params Expression<Func<T, object>>[] includeExpressions)
        {
            var includeProps = includeExpressions?.Select(GetPropertyName).ToHashSet();
            CopyChangedProperties(source, destination, null, includeProps, true);
        }

        /// <summary>
        /// Copy properties với đầy đủ tùy chọn
        /// </summary>
        /// <param name="ignoreSubSonicProperties">Có bỏ qua các thuộc tính mặc định của SubSonic không (mặc định: true)</param>
        public static void CopyChangedPropertiesAdvanced<T>(T source, T destination,
            string[] excludeProperties = null,
            Expression<Func<T, object>>[] excludeExpressions = null,
            Expression<Func<T, object>>[] includeExpressions = null,
            bool ignoreSubSonicProperties = true)
        {
            var excludeProps = new HashSet<string>();

            if (excludeProperties != null)
                excludeProps.UnionWith(excludeProperties);

            if (excludeExpressions != null)
                excludeProps.UnionWith(excludeExpressions.Select(GetPropertyName));

            var includeProps = includeExpressions?.Select(GetPropertyName).ToHashSet();

            CopyChangedProperties(source, destination, excludeProps, includeProps, ignoreSubSonicProperties);
        }

        /// <summary>
        /// Copy chỉ các thuộc tính business logic (bỏ qua tất cả SubSonic metadata)
        /// </summary>
        public static void CopyBusinessProperties<T>(T source, T destination, params Expression<Func<T, object>>[] additionalExcludes)
        {
            var excludeProps = additionalExcludes?.Select(GetPropertyName).ToHashSet();
            CopyChangedProperties(source, destination, excludeProps, null, true);
        }
        public static void CopyChangedPropertiesAdvanced<T>(
    T source,
    T destination,
    HashSet<string> excludeProperties,
    HashSet<string> includeProperties,
    bool ignoreSubSonicProperties = true,
    Func<PropertyInfo, object, bool> shouldCopy = null)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException("Source or Destination is null");

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (prop.PropertyType.Namespace == "SweetSoft.QLDA.DataAccess")
                    continue;
                if (prop.GetCustomAttribute<IgnoreCopyAttribute>() != null)
                    continue;
                if (ignoreSubSonicProperties && (SubSonicDefaultProperties.Contains(prop.Name) || IsSubSonicRelatedProperty(prop.Name)))
                    continue;
                if (excludeProperties != null && excludeProperties.Contains(prop.Name))
                    continue;
                if (includeProperties != null && !includeProperties.Contains(prop.Name))
                    continue;

                try
                {
                    var sourceValue = prop.GetValue(source);
                    var destValue = prop.GetValue(destination);

                    if (shouldCopy != null && !shouldCopy(prop, sourceValue))
                        continue;

                    if (!Equals(sourceValue, destValue))
                    {
                        prop.SetValue(destination, sourceValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying property {prop.Name}: {ex.Message}");
                }
            }
        }

        private static void CopyChangedProperties<T>(T source, T destination,
            HashSet<string> excludeProperties, HashSet<string> includeProperties, bool ignoreSubSonicProperties = true)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException("Source or Destination is null");

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                if (prop.PropertyType.Namespace == "SweetSoft.QLDA.DataAccess")
                    continue;
                if (prop.GetCustomAttribute<IgnoreCopyAttribute>() != null)
                    continue;

                if (ignoreSubSonicProperties && SubSonicDefaultProperties.Contains(prop.Name))
                    continue;

                if (ignoreSubSonicProperties && IsSubSonicRelatedProperty(prop.Name))
                    continue;

                if (excludeProperties != null && excludeProperties.Contains(prop.Name))
                    continue;

                if (includeProperties != null && !includeProperties.Contains(prop.Name))
                    continue;

                try
                {
                    var sourceValue = prop.GetValue(source);
                    var destValue = prop.GetValue(destination);

                    if (!Equals(sourceValue, destValue))
                    {
                        prop.SetValue(destination, sourceValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying property {prop.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem thuộc tính có phải là thuộc tính liên quan đến SubSonic không
        /// </summary>
        private static bool IsSubSonicRelatedProperty(string propertyName)
        {
            // Các pattern thường thấy trong SubSonic
            return propertyName.EndsWith("Collection") ||
                   propertyName.EndsWith("Records") ||
                   propertyName.EndsWith("List") ||
                   propertyName.StartsWith("Get") ||
                   propertyName.StartsWith("Set") ||
                   propertyName.Contains("Foreign") ||
                   propertyName.Contains("TableName") ||
                   propertyName.Contains("Schema");
        }

        private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (expression.Body is UnaryExpression unaryExpression &&
                unaryExpression.Operand is MemberExpression memberExpr)
            {
                return memberExpr.Member.Name;
            }

            throw new ArgumentException("Expression must be a property access");
        }
    }

    public static class ObjectExtensions
    {
        public static void CopyChangedPropertiesTo<T>(this T source, T destination, params string[] excludeProperties)
        {
            ObjectHelper.CopyChangedProperties(source, destination, excludeProperties);
        }

        public static void CopyChangedPropertiesTo<T>(this T source, T destination, params Expression<Func<T, object>>[] excludeExpressions)
        {
            ObjectHelper.CopyChangedProperties(source, destination, excludeExpressions);
        }
    }

}
