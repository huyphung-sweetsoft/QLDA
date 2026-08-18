using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class BrandSchema : SchemaEntity
    {
        public BrandSchema() : base("Brand")
        {
        }

        public BrandSchema WithName(string name)
        {
            return Set<BrandSchema>("name", name);
        }
    }
}
