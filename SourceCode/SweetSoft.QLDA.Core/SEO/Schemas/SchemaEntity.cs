using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    /// <summary>
    /// Represents the base class for Schema.org entities.
    /// </summary>
    public abstract class SchemaEntity
    {
        internal const string DefaultContext = "https://schema.org";

        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>(StringComparer.Ordinal);

        protected SchemaEntity(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Schema type cannot be null or whitespace.", nameof(type));
            }

            Type = type;
        }

        public string Type { get; }

        protected void SetProperty(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Property name cannot be null or whitespace.", nameof(propertyName));
            }

            if (IsNullOrEmpty(value))
            {
                _properties.Remove(propertyName);
                return;
            }

            if (value is IEnumerable enumerable && !(value is string))
            {
                var items = new List<object>();
                foreach (var item in enumerable)
                {
                    items.Add(item);
                }

                if (items.Count == 0)
                {
                    _properties.Remove(propertyName);
                    return;
                }

                _properties[propertyName] = items;
                return;
            }

            _properties[propertyName] = value;
        }

        private static bool IsNullOrEmpty(object value)
        {
            if (value == null)
            {
                return true;
            }

            var stringValue = value as string;
            if (stringValue != null)
            {
                return string.IsNullOrWhiteSpace(stringValue);
            }

            if (value is ICollection collection)
            {
                return collection.Count == 0;
            }

            if (value is IEnumerable enumerable && !(value is string))
            {
                foreach (var _ in enumerable)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public virtual Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["@type"] = Type
            };

            foreach (var kvp in _properties)
            {
                var normalizedValue = NormalizeValue(kvp.Value);
                if (normalizedValue != null)
                {
                    result[kvp.Key] = normalizedValue;
                }
            }

            return result;
        }

        protected static object NormalizeValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            var entity = value as SchemaEntity;
            if (entity != null)
            {
                return entity.ToDictionary();
            }

            if (value is IEnumerable enumerable && !(value is string))
            {
                var items = new List<object>();
                foreach (var item in enumerable)
                {
                    var normalized = NormalizeValue(item);
                    if (normalized != null)
                    {
                        items.Add(normalized);
                    }
                }

                if (items.Count == 0)
                {
                    return null;
                }

                return items;
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is Uri uri)
            {
                return uri.ToString();
            }

            return value;
        }

        protected TEntity Set<TEntity>(string propertyName, object value) where TEntity : SchemaEntity
        {
            SetProperty(propertyName, value);
            return (TEntity)this;
        }
    }
}
