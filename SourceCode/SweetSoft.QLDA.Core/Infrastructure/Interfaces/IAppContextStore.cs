using SweetSoft.QLDA.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    internal interface IAppContextStore
    {
        SweetContext Get();
        void Set(SweetContext context);
        void Clear();
    }
}
