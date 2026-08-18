using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class BreadcrumbListSchema : SchemaEntity
    {
        public BreadcrumbListSchema() : base("BreadcrumbList")
        {
        }

        public BreadcrumbListSchema WithItems(params ListItemSchema[] items)
        {
            return Set<BreadcrumbListSchema>("itemListElement", items);
        }
    }
}
