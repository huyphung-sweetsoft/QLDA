using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;

namespace SweetSoft.QLDA.Core.Helpers
{
    public class CommonHelpers
    {
        #region QueryForm

        /// <summary>
        /// Gets query form value by name
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query Form value</returns>
        public static string QueryForm(string Name)
        {
            string result = string.Empty;
            if (HttpContext.Current != null && HttpContext.Current.Request.Form[Name] != null)
                result = HttpContext.Current.Request.Form[Name].ToString();
            return result;
        }

        /// <summary>
        /// Gets boolean value from Query Form 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query Form value</returns>
        public static bool QueryFormBool(string Name)
        {
            string resultStr = QueryForm(Name).ToUpperInvariant();
            return (resultStr == "YES" || resultStr == "TRUE" || resultStr == "1");
        }

        /// <summary>
        /// Gets integer value from Query Form 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query Form value</returns>
        public static int QueryFormInt(string Name)
        {
            string resultStr = QueryForm(Name).ToUpperInvariant();
            int result;
            Int32.TryParse(resultStr, out result);
            return result;
        }

        public static byte QueryFormByte(string Name)
        {
            string resultStr = QueryForm(Name).ToUpperInvariant();
            byte result = byte.MaxValue;
            if (!string.IsNullOrEmpty(resultStr))
                result = byte.Parse(resultStr);
            return result;
        }

        /// <summary>
        /// Gets integer value from Query Form 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query Form value</returns>
        public static long QueryFormLong(string Name)
        {
            string resultStr = QueryForm(Name).ToUpperInvariant();
            long result;
            Int64.TryParse(resultStr, out result);
            return result;
        }

        /// <summary>
        /// Gets integer value from Query Form 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <param name="DefaultValue">Default value</param>
        /// <returns>Query Form value</returns>
        public static int QueryFormInt(string Name, int DefaultValue)
        {
            string resultStr = QueryForm(Name).ToUpperInvariant();
            if (resultStr.Length > 0)
            {
                return Int32.Parse(resultStr);
            }
            return DefaultValue;
        }

        /// <summary>
        /// Gets GUID value from Query Form 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query Form value</returns>
        public static Guid? QueryFormGUID(string Name)
        {
            string resultStr = QueryForm(Name).ToUpperInvariant();
            Guid? result = null;
            try
            {
                result = new Guid(resultStr);
            }
            catch
            {
            }
            return result;
        }


        #endregion

        #region QueryString

        /// <summary>
        /// Gets query string value by name
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query string value</returns>
        public static string QueryString(string Name)
        {
            try
            {
                string result = string.Empty;
                if (HttpContext.Current == null || HttpContext.Current.Request == null)
                    return string.Empty;

                if (HttpContext.Current.Request.QueryString[Name] != null)
                    result = HttpContext.Current.Request.QueryString[Name].ToString();
                else
                {
                    if (HttpContext.Current.Request.UrlReferrer == null)
                        return string.Empty;

                    string queryParams = HttpContext.Current.Request.UrlReferrer.Query;
                    if (string.IsNullOrEmpty(queryParams))
                        return string.Empty;
                    Dictionary<string, string> dic = Regex.Matches(queryParams, "([^?=&]+)(=([^&]*))?").Cast<Match>().ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);
                    if (dic == null)
                        return string.Empty;
                    if (!dic.TryGetValue(Name, out result))
                        return string.Empty;
                    return result;
                }
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',')[0];
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets boolean value from query string 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query string value</returns>
        public static bool QueryStringBool(string Name)
        {
            try
            {
                string resultStr = QueryString(Name).ToUpperInvariant();
                if (string.IsNullOrEmpty(resultStr))
                    return false;
                return (resultStr == "YES" || resultStr == "TRUE" || resultStr == "1");
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Gets integer value from query string 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query string value</returns>
        public static int QueryStringInt(string Name)
        {
            string resultStr = QueryString(Name).ToUpperInvariant();
            int result = -1;
            Int32.TryParse(resultStr, out result);
            return result;
        }

        public static string GetRelativeClientPath(Page page, string virtualPath)
        {
            string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
            if (applicationVirtualPath == "/")
                applicationVirtualPath = string.Empty;

            if (virtualPath.StartsWith("/") || string.IsNullOrEmpty(virtualPath))
                return string.Format("{0}{1}", applicationVirtualPath, virtualPath);
            else if (virtualPath.StartsWith("~"))
            {
                string relativeSourceDirectory;
                if (page == null)
                    relativeSourceDirectory = string.Empty;
                else
                    relativeSourceDirectory = page.AppRelativeTemplateSourceDirectory.Substring(1).TrimEnd('/');
                if (relativeSourceDirectory == "/")
                    relativeSourceDirectory = string.Empty;
                return string.Format("{0}{1}{2}", applicationVirtualPath, relativeSourceDirectory, virtualPath.TrimStart('~'));
            }
            else
                return string.Format("{0}/{1}", applicationVirtualPath, virtualPath);
        }

        /// <summary>
        /// Gets integer value from query string 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <param name="DefaultValue">Default value</param>
        /// <returns>Query string value</returns>
        public static int QueryStringInt(string queryName, int defaultValue)
        {
            string resultStr = QueryString(queryName).ToUpperInvariant();
            if (resultStr.Length > 0)
            {
                return Int32.Parse(resultStr);
            }
            return defaultValue;
        }

        public static byte QueryStringByte(string Name)
        {
            string resultStr = QueryString(Name).ToUpperInvariant();
            byte result = byte.MaxValue;
            if (!string.IsNullOrEmpty(resultStr))
                result = byte.Parse(resultStr);
            return result;
        }

        /// <summary>
        /// Gets integer value from query string 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query string value</returns>
        public static long QueryStringLong(string Name)
        {
            string resultStr = QueryString(Name).ToUpperInvariant();
            long result;
            Int64.TryParse(resultStr, out result);
            return result;
        }

        public static DateTime QueryStringDateTime(string Name)
        {
            string resultStr = QueryString(Name);
            DateTime dt = DateTime.MinValue;
            if(!DateTimeHelper.IsDate(resultStr, ref dt))
                return DateTime.MinValue;
            return dt;
        }

        /// <summary>
        /// Gets GUID value from query string 
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <returns>Query string value</returns>
        public static Guid QueryStringGUID(string Name)
        {
            string resultStr = QueryString(Name).ToUpperInvariant();
            Guid result = Guid.Empty;
            try
            {
                result = new Guid(resultStr);
            }
            catch
            {
                result = Guid.Empty;
            }

            return result;
        }

        #endregion


        /// <summary>
        /// Check file is opened, return true if file is opened 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileIsOpen(string filePath)
        {
            bool results = false;
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    try
                    {
                        stream.ReadByte();
                    }
                    catch (IOException)
                    {
                        results = true;
                    }
                    finally
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            catch (IOException)
            {
                results = true;  //file is opened at another location
            }

            return results;
        }

        /// <summary>
        /// Status of object in Project
        /// </summary>
        /// <param name="objStatus"></param>
        /// <returns></returns>

        public static string GetFullApplicationPath()
        {
            return HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath;
        }

        /// <summary>
        /// Gets this page name
        /// </summary>
        /// <returns></returns>
        public static string GetThisPageURL(bool includeQueryString)
        {
            string URL = string.Empty;
            if (HttpContext.Current == null)
                return URL;

            if (includeQueryString)
            {
                string storeHost = GetSiteRoot();
                if (storeHost.EndsWith("/"))
                    storeHost = storeHost.Substring(0, storeHost.Length - 1);
                URL = storeHost + HttpContext.Current.Request.RawUrl;
            }
            else
            {
                URL = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);
            }
            return URL;
        }

        public static string GetSiteRoot()
        {
            string port = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
            if (port == null || port == "80" || port == "443")
                port = "";
            else
                port = ":" + port;

            string protocol = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"];
            if (protocol == null || protocol == "0")
                protocol = "http://";
            else
                protocol = "https://";

            string sOut = protocol + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + port + System.Web.HttpContext.Current.Request.ApplicationPath;

            if (sOut.EndsWith("/"))
            {
                sOut = sOut.Substring(0, sOut.Length - 1);
            }

            return sOut;
        }

        public static IEnumerable<Control> GetAllControlByType(Control parent, Type type)
        {
            var controls = parent.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAllControlByType(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type
                                        || c.GetType().BaseType == type
                                        || c.GetType().BaseType.BaseType == type);
        }
        public static Control GetControlById(Control parent, string clientId)
        {
            Control ctl = parent;
            var ctls = new LinkedList<Control>();
            while (ctl != null)
            {
                if (ctl.ClientID == clientId)
                    return ctl;
                foreach (Control child in ctl.Controls)
                {
                    if (child.ClientID == clientId)
                        return child;
                    if (child.HasControls())
                        ctls.AddLast(child);
                }
                if (ctls.First != null)
                {
                    ctl = ctls.First.Value;
                    ctls.Remove(ctl);
                }
                else return null;
            }
            return null;
        }
        public static string TrimLongString(string text, int numberOfCharacters)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length <= numberOfCharacters)
                return text;

            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words[0].Length > numberOfCharacters)
                return words[0];
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if ((sb + word).Length > numberOfCharacters)
                    return string.Format("{0}...", sb.ToString().TrimEnd(' '));
                sb.Append(word + " ");
            }
            return string.Format("{0}...", sb.ToString().TrimEnd(' '));
        }

        private static string _frontEndUrl = "";
        public static string HostPathFrontEnd
        {
            get
            {
                if (!string.IsNullOrEmpty(_frontEndUrl))
                    return _frontEndUrl;
                _frontEndUrl = AppSettingHelpers.GetSetting<string>("WEB_HOST_PATH");
                if (string.IsNullOrEmpty(_frontEndUrl))
                    _frontEndUrl = GetHostPath();
                string result = string.Empty;
                if (_frontEndUrl.StartsWith("https://") || _frontEndUrl.StartsWith("http://"))
                    result = _frontEndUrl;
                else
                    result = "http://" + _frontEndUrl;
                if (!result.EndsWith("/"))
                    result += "/";
                return result;
            }
        }
        public static string GetHostPath()
        {
            string httpHost = string.Empty; // ServerVariables("HTTP_HOST");
            try
            {
                return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/";
            }
            catch
            {
                httpHost = ServerVariables("HTTP_HOST");
            }
            if (string.IsNullOrEmpty(httpHost))
                httpHost = "localhost";
            string result = string.Empty;
            if (httpHost.StartsWith("https://") || httpHost.StartsWith("http://"))
                result = httpHost;
            else
                result = "http://" + httpHost;
            if (!result.EndsWith("/"))
                result += "/";
            return result;
        }
        public static string GetHostPath(bool hasEndHash = true, string subApp = "")
        {
            if (!string.IsNullOrEmpty(subApp))
                hasEndHash = true;

            string httpHost = string.Empty;
            try
            {
                httpHost = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                if (hasEndHash) httpHost += "/";
            }
            catch { }

            return httpHost + subApp;
        }
        public static string GetAPIHostPath()
        {
            string httpHost = AppSettingHelpers.GetSetting<string>("API_HOST_PATH");
            if (string.IsNullOrEmpty(httpHost))
                httpHost = GetHostPath();
            string result = string.Empty;
            if (httpHost.StartsWith("https://") || httpHost.StartsWith("http://"))
                result = httpHost;
            else
                result = "http://" + httpHost;
            if (!result.EndsWith("/"))
                result += "/";
            return result;
        }
        public static string ServerVariables(string Name)
        {
            string tmpS = String.Empty;
            try
            {
                if (HttpContext.Current.Request.ServerVariables[Name] != null)
                {
                    tmpS = HttpContext.Current.Request.ServerVariables[Name];
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.ApplicationPath))
                        tmpS += HttpContext.Current.Request.ApplicationPath;
                }
            }
            catch
            {
                tmpS = String.Empty;
            }
            return tmpS;
        }
    }
}
