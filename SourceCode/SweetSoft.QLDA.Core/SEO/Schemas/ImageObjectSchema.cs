using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class ImageObjectSchema : SchemaEntity
    {
        public ImageObjectSchema() : base("ImageObject")
        {
        }

        public ImageObjectSchema WithUrl(string url)
        {
            return Set<ImageObjectSchema>("url", url);
        }

        public ImageObjectSchema WithHeight(int height)
        {
            return Set<ImageObjectSchema>("height", height);
        }

        public ImageObjectSchema WithWidth(int width)
        {
            return Set<ImageObjectSchema>("width", width);
        }

        public ImageObjectSchema WithCaption(string caption)
        {
            return Set<ImageObjectSchema>("caption", caption);
        }
    }
}
