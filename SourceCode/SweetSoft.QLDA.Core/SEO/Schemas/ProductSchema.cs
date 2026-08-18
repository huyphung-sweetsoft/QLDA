using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class ProductSchema : SchemaEntity
    {
        public ProductSchema() : base("Product")
        {
        }

        public ProductSchema WithName(string name)
        {
            return Set<ProductSchema>("name", name);
        }

        public ProductSchema WithDescription(string description)
        {
            return Set<ProductSchema>("description", description);
        }

        public ProductSchema WithSku(string sku)
        {
            return Set<ProductSchema>("sku", sku);
        }

        public ProductSchema WithBrand(BrandSchema brand)
        {
            return Set<ProductSchema>("brand", brand);
        }

        public ProductSchema WithOffers(params OfferSchema[] offers)
        {
            return Set<ProductSchema>("offers", offers);
        }

        public ProductSchema WithAggregateRating(AggregateRatingSchema rating)
        {
            return Set<ProductSchema>("aggregateRating", rating);
        }

        public ProductSchema WithReview(ReviewSchema review)
        {
            return Set<ProductSchema>("review", review);
        }

        public ProductSchema WithReviews(IEnumerable<ReviewSchema> reviews)
        {
            return Set<ProductSchema>("review", reviews);
        }
    }
}
