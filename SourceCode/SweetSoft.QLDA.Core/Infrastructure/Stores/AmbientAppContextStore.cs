using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Stores
{
    internal sealed class AmbientAppContextStore : IAppContextStore
    {
        private static readonly AsyncLocal<SweetContext> AmbientContext = new AsyncLocal<SweetContext>();

        public SweetContext Get()
        {
            return AmbientContext.Value;
        }

        public void Set(SweetContext context)
        {
            AmbientContext.Value = context;
        }

        public void Clear()
        {
            AmbientContext.Value = null;
        }
    }
}
