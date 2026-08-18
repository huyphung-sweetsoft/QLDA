using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    /// <summary>
    /// A generic Schema.org entity implementation that can be used for quick extensions.
    /// </summary>
    public sealed class GenericSchemaEntity : SchemaEntity
    {
        public GenericSchemaEntity(string type) : base(type)
        {
        }

        public GenericSchemaEntity WithProperty(string name, object value)
        {
            return Set<GenericSchemaEntity>(name, value);
        }
    }
}
