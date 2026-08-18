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
    internal sealed class BackgroundRequestEnvironment : IRequestEnvironment
    {
        private readonly IDictionary _items = new HybridDictionary();
        private readonly string _siteUrl;
        private readonly string _rootPath;
        private Uri _currentUri;

        public BackgroundRequestEnvironment(Uri baseUri = null, string siteUrl = null)
        {
            _currentUri = baseUri ?? RequestUtilities.DefaultUri;
            _siteUrl = string.IsNullOrEmpty(siteUrl) ? RequestUtilities.BuildSiteUrl(_currentUri) : siteUrl;
            _rootPath = AppDomain.CurrentDomain.BaseDirectory.Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        public IDictionary Items => _items;

        public bool IsWebRequest => false;

        public HttpContext Context => null;

        public string SiteUrl => _siteUrl;

        public Uri CurrentUri
        {
            get => _currentUri ?? RequestUtilities.DefaultUri;
            set => _currentUri = value ?? RequestUtilities.DefaultUri;
        }

        public string HostPath => RequestUtilities.BuildHostPath(CurrentUri);

        public string MapPath(string path)
        {
            return PhysicalPath(path);
        }

        public string PhysicalPath(string path)
        {
            return RequestUtilities.ResolvePhysicalPath(_rootPath, path);
        }

        public bool HasHttpContext => false;

        public string GetUserIpAddress() => RequestUtilities.DefaultIpAddress;

        public string GetUserAgent() => string.Empty;
    }
}
