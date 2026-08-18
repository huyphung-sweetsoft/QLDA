using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class ArticleSchema : SchemaEntity
    {
        public ArticleSchema() : base("Article")
        {
        }

        public ArticleSchema WithHeadline(string headline)
        {
            return Set<ArticleSchema>("headline", headline);
        }

        public ArticleSchema WithDescription(string description)
        {
            return Set<ArticleSchema>("description", description);
        }

        public ArticleSchema WithAuthor(PersonSchema author)
        {
            return Set<ArticleSchema>("author", author);
        }

        public ArticleSchema WithPublisher(OrganizationSchema publisher)
        {
            return Set<ArticleSchema>("publisher", publisher);
        }

        public ArticleSchema WithDatePublished(DateTimeOffset date)
        {
            return Set<ArticleSchema>("datePublished", date);
        }

        public ArticleSchema WithDateModified(DateTimeOffset date)
        {
            return Set<ArticleSchema>("dateModified", date);
        }

        public ArticleSchema WithImage(ImageObjectSchema image)
        {
            return Set<ArticleSchema>("image", image);
        }

        public ArticleSchema WithMainEntityOfPage(string url)
        {
            return Set<ArticleSchema>("mainEntityOfPage", url);
        }
    }

}
