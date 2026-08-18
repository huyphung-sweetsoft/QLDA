using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.WebRequest
{
    public static class UserAgentHelper
    {
        public static DeviceInfo GetDeviceInfo(HttpRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserAgent))
                return new DeviceInfo();

            return ParseUserAgent(request.UserAgent);
        }

        public static DeviceInfo ParseUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return new DeviceInfo();

            string os = DetectOS(userAgent);
            string browser = DetectBrowser(userAgent, out string version);

            return new DeviceInfo
            {
                OS = os,
                Browser = browser,
                BrowserVersion = version,
                Device = userAgent.Contains("Mobile") ? "Mobile" : "Desktop"
            };
        }

        private static string DetectOS(string ua)
        {
            if (ua.Contains("Windows NT 10.0")) return "Windows 10";
            if (ua.Contains("Windows NT 6.3")) return "Windows 8.1";
            if (ua.Contains("Windows NT 6.2")) return "Windows 8";
            if (ua.Contains("Windows NT 6.1")) return "Windows 7";
            if (ua.Contains("Windows NT 6.0")) return "Windows Vista";
            if (ua.Contains("Windows NT 5.1")) return "Windows XP";
            if (ua.Contains("Mac OS X")) return "macOS";
            if (ua.Contains("Android")) return "Android";
            if (ua.Contains("iPhone")) return "iOS (iPhone)";
            if (ua.Contains("iPad")) return "iOS (iPad)";
            if (ua.Contains("Linux")) return "Linux";

            return "Unknown";
        }

        private static string DetectBrowser(string ua, out string version)
        {
            version = string.Empty;

            // Chrome (exclude Edge Chromium)
            var m = Regex.Match(ua, @"Chrome/([\d\.]+)", RegexOptions.IgnoreCase);
            if (m.Success && !ua.Contains("Edg/"))
            {
                version = m.Groups[1].Value;
                return "Chrome";
            }

            // Edge (Chromium)
            m = Regex.Match(ua, @"Edg/([\d\.]+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                version = m.Groups[1].Value;
                return "Edge";
            }

            // Firefox
            m = Regex.Match(ua, @"Firefox/([\d\.]+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                version = m.Groups[1].Value;
                return "Firefox";
            }

            // Safari (but not Chrome)
            m = Regex.Match(ua, @"Version/([\d\.]+).*Safari", RegexOptions.IgnoreCase);
            if (m.Success && ua.Contains("Safari") && !ua.Contains("Chrome"))
            {
                version = m.Groups[1].Value;
                return "Safari";
            }

            // IE (11 trở xuống)
            m = Regex.Match(ua, @"MSIE\s([\d\.]+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                version = m.Groups[1].Value;
                return "Internet Explorer";
            }

            m = Regex.Match(ua, @"rv:([\d\.]+)\) like Gecko", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                version = m.Groups[1].Value;
                return "Internet Explorer";
            }

            return "Unknown";
        }
    }

    public class DeviceInfo
    {
        public string OS { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string BrowserVersion { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
    }
}
