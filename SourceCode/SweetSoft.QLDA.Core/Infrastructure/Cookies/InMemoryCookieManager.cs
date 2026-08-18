using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Cookies
{
    internal sealed class InMemoryCookieManager : ICookieManager
    {
        private readonly Dictionary<string, string> _cookies = new Dictionary<string, string>(StringComparer.Ordinal);

        public string Get(string key)
        {
            return _cookies.TryGetValue(key, out var value) ? value : null;
        }

        public void Set(string key, string value, DateTime expires)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (value == null)
            {
                _cookies.Remove(key);
            }
            else
            {
                _cookies[key] = value;
            }
        }
    }

}
