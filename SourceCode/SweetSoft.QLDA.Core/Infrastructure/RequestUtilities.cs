using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure
{
    internal static class RequestUtilities
    {
        internal static readonly Uri DefaultUri = new Uri("https://localhost/");
        internal const string DefaultIpAddress = "127.0.0.1";

        internal static string BuildHostPath(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }

            var portInfo = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
            return $"{uri.Scheme}://{uri.Host}{portInfo}";
        }

        internal static string NormalizeVirtualPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path
                .Replace("~", string.Empty)
                .Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        internal static string ResolvePhysicalPath(string rootPath, string path)
        {
            var sanitizedRoot = (rootPath ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar);
            var sanitizedPath = NormalizeVirtualPath(path).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(sanitizedRoot, sanitizedPath);
        }

        internal static string BuildSiteUrl(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }

            var hostName = uri.Host.Replace("www.", string.Empty);
            var applicationPath = uri.AbsolutePath;

            if (applicationPath.EndsWith("/", StringComparison.Ordinal))
            {
                applicationPath = applicationPath.TrimEnd('/');
            }

            if (string.IsNullOrEmpty(applicationPath))
            {
                return hostName;
            }

            return hostName + applicationPath;
        }
    }

}
