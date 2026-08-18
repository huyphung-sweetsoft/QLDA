using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class PostalAddressSchema : SchemaEntity
    {
        public PostalAddressSchema() : base("PostalAddress")
        {
        }

        public PostalAddressSchema WithStreetAddress(string street)
        {
            return Set<PostalAddressSchema>("streetAddress", street);
        }

        public PostalAddressSchema WithAddressLocality(string locality)
        {
            return Set<PostalAddressSchema>("addressLocality", locality);
        }

        public PostalAddressSchema WithAddressRegion(string region)
        {
            return Set<PostalAddressSchema>("addressRegion", region);
        }

        public PostalAddressSchema WithPostalCode(string postalCode)
        {
            return Set<PostalAddressSchema>("postalCode", postalCode);
        }

        public PostalAddressSchema WithAddressCountry(string country)
        {
            return Set<PostalAddressSchema>("addressCountry", country);
        }
    }
}
