using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.Infrastructure.Cookies
{
    internal sealed class HttpCookieManager : ICookieManager
    {
        private readonly HttpContext _context;

        public HttpCookieManager(HttpContext context)
        {
            _context = context;
        }

        public string Get(string key)
        {
            return _context?.Request?.Cookies[key]?.Value;
        }

        public void Set(string key, string value, DateTime expires)
        {
            if (_context?.Response == null)
            {
                return;
            }

            var cookie = _context.Request?.Cookies[key] ?? new HttpCookie(key);
            cookie.Value = value;
            cookie.Expires = expires;
            _context.Response.Cookies.Set(cookie);
        }
    }

}
