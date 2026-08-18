using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class ContactPointSchema : SchemaEntity
    {
        public ContactPointSchema() : base("ContactPoint")
        {
        }

        public ContactPointSchema WithTelephone(string telephone)
        {
            return Set<ContactPointSchema>("telephone", telephone);
        }

        public ContactPointSchema WithContactType(string contactType)
        {
            return Set<ContactPointSchema>("contactType", contactType);
        }

        public ContactPointSchema WithAreaServed(params string[] areas)
        {
            return Set<ContactPointSchema>("areaServed", areas);
        }

        public ContactPointSchema WithAvailableLanguage(params string[] languages)
        {
            return Set<ContactPointSchema>("availableLanguage", languages);
        }
    }
}
