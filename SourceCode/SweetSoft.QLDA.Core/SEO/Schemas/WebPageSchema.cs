using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class WebPageSchema : SchemaEntity
    {
        public WebPageSchema() : base("WebPage")
        {
        }

        public WebPageSchema WithName(string name)
        {
            return Set<WebPageSchema>("name", name);
        }

        public WebPageSchema WithUrl(string url)
        {
            return Set<WebPageSchema>("url", url);
        }

        public WebPageSchema WithHeadline(string headline)
        {
            return Set<WebPageSchema>("headline", headline);
        }

        public WebPageSchema WithDescription(string description)
        {
            return Set<WebPageSchema>("description", description);
        }

        public WebPageSchema WithBreadcrumb(BreadcrumbListSchema breadcrumb)
        {
            return Set<WebPageSchema>("breadcrumb", breadcrumb);
        }

        public WebPageSchema WithIsPartOf(WebSiteSchema website)
        {
            return Set<WebPageSchema>("isPartOf", website);
        }

        public WebPageSchema WithPrimaryImage(ImageObjectSchema image)
        {
            return Set<WebPageSchema>("primaryImageOfPage", image);
        }
    }
}
