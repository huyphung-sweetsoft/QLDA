using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Stores
{
    internal sealed class CompositeAppContextStore : IAppContextStore
    {
        private readonly IAppContextStore[] _stores;

        public CompositeAppContextStore(params IAppContextStore[] stores)
        {
            _stores = stores ?? Array.Empty<IAppContextStore>();
        }

        public SweetContext Get()
        {
            foreach (var store in _stores)
            {
                var context = store.Get();
                if (context != null)
                {
                    return context;
                }
            }

            return null;
        }

        public SweetContext GetOrCreate(Func<SweetContext> factory)
        {
            var context = Get();
            if (context != null)
            {
                return context;
            }

            context = factory();
            Set(context);
            return context;
        }

        public void Set(SweetContext context)
        {
            foreach (var store in _stores)
            {
                store.Set(context);
            }
        }

        public void Clear()
        {
            foreach (var store in _stores)
            {
                store.Clear();
            }
        }
    }
}
