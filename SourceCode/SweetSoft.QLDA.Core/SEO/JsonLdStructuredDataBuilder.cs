using SweetSoft.QLDA.Core.SEO.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO
{
    /// <summary>
    /// Provides a fluent API for composing JSON-LD structured data payloads that comply with Google Search guidelines.
    /// </summary>
    public sealed class JsonLdStructuredDataBuilder
    {
        private readonly List<SchemaEntity> _entities = new List<SchemaEntity>();
        private readonly string _context;
        private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        public JsonLdStructuredDataBuilder(string context = SchemaEntity.DefaultContext)
        {
            if (string.IsNullOrWhiteSpace(context))
            {
                throw new ArgumentException("Context cannot be null or whitespace.", nameof(context));
            }

            _context = context;
        }

        /// <summary>
        /// Adds a schema entity to the JSON-LD document.
        /// </summary>
        public JsonLdStructuredDataBuilder AddEntity(SchemaEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _entities.Add(entity);
            return this;
        }

        /// <summary>
        /// Replaces the serializer options used when rendering JSON-LD.
        /// </summary>
        public JsonLdStructuredDataBuilder WithSerializerOptions(JsonSerializerOptions options)
        {
            _serializerOptions = options ?? throw new ArgumentNullException(nameof(options));
            return this;
        }

        /// <summary>
        /// Converts the composed entities to a JSON-LD payload.
        /// </summary>
        public string Build()
        {
            if (_entities.Count == 0)
            {
                throw new InvalidOperationException("At least one schema entity is required to build JSON-LD.");
            }

            object payload;

            if (_entities.Count == 1)
            {
                var entity = _entities[0].ToDictionary();
                entity["@context"] = _context;
                payload = entity;
            }
            else
            {
                payload = new Dictionary<string, object>
                {
                    ["@context"] = _context,
                    ["@graph"] = _entities.Select(e => e.ToDictionary()).ToList()
                };
            }

            return JsonSerializer.Serialize(payload, _serializerOptions);
        }

        /// <summary>
        /// Builds the JSON-LD payload and wraps it with the appropriate script tag for Web Forms markup.
        /// </summary>
        public string BuildScriptTag()
        {
            var json = Build();
            return "<script type=\"application/ld+json\">\n" + json + "\n</script>";
        }

        /// <summary>
        /// Creates a JSON-LD payload from a single entity without explicitly instantiating a builder.
        /// </summary>
        public static string BuildSingle(SchemaEntity entity)
        {
            return new JsonLdStructuredDataBuilder().AddEntity(entity).Build();
        }
    }
}
