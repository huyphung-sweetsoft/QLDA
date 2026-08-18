using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class ListItemSchema : SchemaEntity
    {
        public ListItemSchema() : base("ListItem")
        {
        }

        public ListItemSchema WithPosition(int position)
        {
            return Set<ListItemSchema>("position", position);
        }

        public ListItemSchema WithName(string name)
        {
            return Set<ListItemSchema>("name", name);
        }

        public ListItemSchema WithItem(string url)
        {
            return Set<ListItemSchema>("item", url);
        }
    }
}
