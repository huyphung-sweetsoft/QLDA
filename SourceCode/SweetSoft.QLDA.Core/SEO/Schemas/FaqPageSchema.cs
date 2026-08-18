using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class FaqPageSchema : SchemaEntity
    {
        public FaqPageSchema() : base("FAQPage")
        {
        }

        public FaqPageSchema WithQuestions(params QuestionSchema[] questions)
        {
            return Set<FaqPageSchema>("mainEntity", questions);
        }
    }
}
