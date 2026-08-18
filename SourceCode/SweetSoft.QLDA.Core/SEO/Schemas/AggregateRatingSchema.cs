using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class AggregateRatingSchema : SchemaEntity
    {
        public AggregateRatingSchema() : base("AggregateRating")
        {
        }

        public AggregateRatingSchema WithRatingValue(decimal value)
        {
            return Set<AggregateRatingSchema>("ratingValue", value);
        }

        public AggregateRatingSchema WithReviewCount(int count)
        {
            return Set<AggregateRatingSchema>("reviewCount", count);
        }

        public AggregateRatingSchema WithBestRating(decimal value)
        {
            return Set<AggregateRatingSchema>("bestRating", value);
        }

        public AggregateRatingSchema WithWorstRating(decimal value)
        {
            return Set<AggregateRatingSchema>("worstRating", value);
        }
    }
}
