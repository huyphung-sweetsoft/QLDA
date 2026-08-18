using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class AnswerSchema : SchemaEntity
    {
        public AnswerSchema() : base("Answer")
        {
        }

        public AnswerSchema WithText(string text)
        {
            return Set<AnswerSchema>("text", text);
        }
    }
}
