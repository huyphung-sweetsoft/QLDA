using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class RatingSchema : SchemaEntity
    {
        public RatingSchema() : base("Rating")
        {
        }

        public RatingSchema WithRatingValue(decimal value)
        {
            return Set<RatingSchema>("ratingValue", value);
        }

        public RatingSchema WithBestRating(decimal value)
        {
            return Set<RatingSchema>("bestRating", value);
        }

        public RatingSchema WithWorstRating(decimal value)
        {
            return Set<RatingSchema>("worstRating", value);
        }
    }
}
