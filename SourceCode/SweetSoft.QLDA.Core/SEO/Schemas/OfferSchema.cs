using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class OfferSchema : SchemaEntity
    {
        public OfferSchema() : base("Offer")
        {
        }

        public OfferSchema WithPrice(decimal price)
        {
            return Set<OfferSchema>("price", price);
        }

        public OfferSchema WithPriceCurrency(string currency)
        {
            return Set<OfferSchema>("priceCurrency", currency);
        }

        public OfferSchema WithAvailability(string availability)
        {
            return Set<OfferSchema>("availability", availability);
        }

        public OfferSchema WithUrl(string url)
        {
            return Set<OfferSchema>("url", url);
        }

        public OfferSchema WithValidFrom(DateTimeOffset date)
        {
            return Set<OfferSchema>("validFrom", date);
        }

        public OfferSchema WithItemCondition(string condition)
        {
            return Set<OfferSchema>("itemCondition", condition);
        }
    }
}
