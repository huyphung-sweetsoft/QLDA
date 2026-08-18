using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class ReviewSchema : SchemaEntity
    {
        public ReviewSchema() : base("Review")
        {
        }

        public ReviewSchema WithAuthor(PersonSchema author)
        {
            return Set<ReviewSchema>("author", author);
        }

        public ReviewSchema WithReviewRating(RatingSchema rating)
        {
            return Set<ReviewSchema>("reviewRating", rating);
        }

        public ReviewSchema WithReviewBody(string reviewBody)
        {
            return Set<ReviewSchema>("reviewBody", reviewBody);
        }

        public ReviewSchema WithName(string name)
        {
            return Set<ReviewSchema>("name", name);
        }

        public ReviewSchema WithDatePublished(DateTimeOffset date)
        {
            return Set<ReviewSchema>("datePublished", date);
        }
    }
}
