using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace SweetSoft.QLDA.Core.Infrastructure.SessionContext
{
    internal sealed class HttpSessionContext : ISessionContext
    {
        private readonly HttpSessionState _session;

        public HttpSessionContext(HttpSessionState session)
        {
            _session = session;
        }

        public bool HasSession => _session != null;

        public HttpSessionState Session => _session;

        public object Get(string key)
        {
            return _session?[key];
        }

        public void Set(string key, object value)
        {
            if (_session == null)
            {
                return;
            }

            _session[key] = value;
        }

        public void Remove(string key)
        {
            _session?.Remove(key);
        }

        public void ClearAll()
        {
            _session?.RemoveAll();
        }
    }
}
