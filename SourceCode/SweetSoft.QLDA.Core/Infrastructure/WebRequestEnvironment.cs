using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.Infrastructure
{
    internal sealed class WebRequestEnvironment : IRequestEnvironment
    {
        private readonly HttpContext _context;
        private readonly IDictionary _items;
        private readonly string _siteUrl;
        private Uri _currentUri;

        public WebRequestEnvironment(HttpContext context)
        {
            _context = context;
            _items = context?.Items ?? new HybridDictionary();
            _currentUri = context?.Request?.Url ?? RequestUtilities.DefaultUri;
            _siteUrl = BuildSiteUrl(context);
        }

        public IDictionary Items => _items;

        public bool IsWebRequest => true;

        public HttpContext Context => _context;

        public string SiteUrl => _siteUrl;

        public Uri CurrentUri
        {
            get => _currentUri ?? RequestUtilities.DefaultUri;
            set => _currentUri = value ?? RequestUtilities.DefaultUri;
        }

        public string HostPath => RequestUtilities.BuildHostPath(CurrentUri);

        public string MapPath(string path)
        {
            if (_context?.Server != null)
            {
                return _context.Server.MapPath(path);
            }

            return PhysicalPath(path);
        }

        public string PhysicalPath(string path)
        {
            var root = AppDomain.CurrentDomain.BaseDirectory.Replace("/", Path.DirectorySeparatorChar.ToString());
            return RequestUtilities.ResolvePhysicalPath(root, path);
        }

        public bool HasHttpContext => _context != null;

        public string GetUserIpAddress()
        {
            var request = _context?.Request;
            if (request == null)
            {
                return RequestUtilities.DefaultIpAddress;
            }

            var ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = request.ServerVariables["REMOTE_ADDR"];
            }

            return string.IsNullOrWhiteSpace(ip) ? RequestUtilities.DefaultIpAddress : ip;
        }

        public string GetUserAgent()
        {
            return _context?.Request?.UserAgent ?? string.Empty;
        }

        private static string BuildSiteUrl(HttpContext context)
        {
            if (context?.Request?.Url == null)
            {
                return string.Empty;
            }

            var hostName = context.Request.Url.Host.Replace("www.", string.Empty);
            var applicationPath = context.Request.ApplicationPath ?? string.Empty;

            if (applicationPath.EndsWith("/", StringComparison.Ordinal))
            {
                applicationPath = applicationPath.TrimEnd('/');
            }

            return string.IsNullOrEmpty(applicationPath) ? hostName : hostName + applicationPath;
        }
    }
}
