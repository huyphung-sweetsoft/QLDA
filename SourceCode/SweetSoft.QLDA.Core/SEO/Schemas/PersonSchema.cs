using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class PersonSchema : SchemaEntity
    {
        public PersonSchema() : base("Person")
        {
        }

        public PersonSchema WithName(string name)
        {
            return Set<PersonSchema>("name", name);
        }

        public PersonSchema WithUrl(string url)
        {
            return Set<PersonSchema>("url", url);
        }

        public PersonSchema WithImage(string image)
        {
            return Set<PersonSchema>("image", image);
        }

        public PersonSchema WithSameAs(params string[] profiles)
        {
            return Set<PersonSchema>("sameAs", profiles);
        }

        public PersonSchema WithJobTitle(string jobTitle)
        {
            return Set<PersonSchema>("jobTitle", jobTitle);
        }
    }
}
