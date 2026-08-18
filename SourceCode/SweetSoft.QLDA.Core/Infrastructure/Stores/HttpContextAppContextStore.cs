using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.Infrastructure.Stores
{
    internal sealed class HttpContextAppContextStore : IAppContextStore
    {
        private const string StoreKey = "ApplicationContextStore";

        public SweetContext Get()
        {
            var httpContext = HttpContext.Current;
            return httpContext?.Items[StoreKey] as SweetContext;
        }

        public void Set(SweetContext context)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return;
            }

            if (context == null)
            {
                httpContext.Items.Remove(StoreKey);
            }
            else
            {
                httpContext.Items[StoreKey] = context;
            }
        }

        public void Clear()
        {
            HttpContext.Current?.Items.Remove(StoreKey);
        }
    }
}
