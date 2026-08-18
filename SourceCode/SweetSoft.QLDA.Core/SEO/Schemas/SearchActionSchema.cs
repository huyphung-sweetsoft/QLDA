using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class SearchActionSchema : SchemaEntity
    {
        public SearchActionSchema() : base("SearchAction")
        {
        }

        public SearchActionSchema WithTarget(string target)
        {
            return Set<SearchActionSchema>("target", target);
        }

        public SearchActionSchema WithQueryInput(string queryInput)
        {
            return Set<SearchActionSchema>("query-input", queryInput);
        }
    }
}
