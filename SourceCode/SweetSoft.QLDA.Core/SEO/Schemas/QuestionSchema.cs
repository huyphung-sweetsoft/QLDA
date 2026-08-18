using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.SEO.Schemas
{
    public sealed class QuestionSchema : SchemaEntity
    {
        public QuestionSchema() : base("Question")
        {
        }

        public QuestionSchema WithName(string name)
        {
            return Set<QuestionSchema>("name", name);
        }

        public QuestionSchema WithAcceptedAnswer(AnswerSchema answer)
        {
            return Set<QuestionSchema>("acceptedAnswer", answer);
        }
    }
}
