using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace SweetSoft.QLDA.Core.Infrastructure.SessionContext
{
    internal sealed class InMemorySessionContext : ISessionContext
    {
        private readonly Dictionary<string, object> _session = new Dictionary<string, object>(StringComparer.Ordinal);

        public bool HasSession => true;

        public HttpSessionState Session => null;

        public object Get(string key)
        {
            return _session.TryGetValue(key, out var value) ? value : null;
        }

        public void Set(string key, object value)
        {
            if (value == null)
            {
                _session.Remove(key);
                return;
            }

            _session[key] = value;
        }

        public void Remove(string key)
        {
            _session.Remove(key);
        }

        public void ClearAll()
        {
            _session.Clear();
        }
    }
}
