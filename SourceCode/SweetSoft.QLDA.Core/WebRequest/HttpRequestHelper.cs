using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.WebRequest
{
    public static class HttpRequestHelper
    {
        public static string GetUserAgent(HttpRequest request)
        {
            if (request == null) return string.Empty;

            return request.UserAgent ?? string.Empty;
        }
    }
}
