using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class OrganizationSchema : SchemaEntity
    {
        public OrganizationSchema() : base("Organization")
        {
        }

        public OrganizationSchema WithName(string name)
        {
            return Set<OrganizationSchema>("name", name);
        }

        public OrganizationSchema WithUrl(string url)
        {
            return Set<OrganizationSchema>("url", url);
        }

        public OrganizationSchema WithLogo(string url)
        {
            return Set<OrganizationSchema>("logo", url);
        }

        public OrganizationSchema WithLogo(ImageObjectSchema logo)
        {
            return Set<OrganizationSchema>("logo", logo);
        }

        public OrganizationSchema WithContactPoints(params ContactPointSchema[] contactPoints)
        {
            return Set<OrganizationSchema>("contactPoint", contactPoints);
        }

        public OrganizationSchema WithSameAs(params string[] profiles)
        {
            return Set<OrganizationSchema>("sameAs", profiles);
        }

        public OrganizationSchema WithAddress(PostalAddressSchema address)
        {
            return Set<OrganizationSchema>("address", address);
        }

        public OrganizationSchema WithFounder(PersonSchema founder)
        {
            return Set<OrganizationSchema>("founder", founder);
        }

        public OrganizationSchema WithFounders(IEnumerable<PersonSchema> founders)
        {
            return Set<OrganizationSchema>("founder", founders);
        }

        public OrganizationSchema WithTaxId(string taxId)
        {
            return Set<OrganizationSchema>("taxID", taxId);
        }
    }
}
