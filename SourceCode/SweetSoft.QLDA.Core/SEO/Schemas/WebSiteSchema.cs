using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class WebSiteSchema : SchemaEntity
    {
        public WebSiteSchema() : base("WebSite")
        {
        }

        public WebSiteSchema WithName(string name)
        {
            return Set<WebSiteSchema>("name", name);
        }

        public WebSiteSchema WithUrl(string url)
        {
            return Set<WebSiteSchema>("url", url);
        }

        public WebSiteSchema WithPotentialAction(SearchActionSchema searchAction)
        {
            return Set<WebSiteSchema>("potentialAction", searchAction);
        }

        public WebSiteSchema WithAlternateNames(params string[] names)
        {
            return Set<WebSiteSchema>("alternateName", names);
        }
    }
}
