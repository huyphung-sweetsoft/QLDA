using SweetSoft.QLDA.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Core.Utils
{
    public class Helpers
    {
        /// <summary>
        /// Private constructor forces singleton.
        /// </summary>
        private Helpers()
        {
        }

        public static string ConvertPostl(string url)
        {
            if (url.StartsWith("/") == true)
                return url;

            if (string.IsNullOrEmpty(url) == false)
            {
                if (url.StartsWith("//") == false
                    && url.ToLower().StartsWith("www") == false
                    && url.ToLower().StartsWith("http") == false)
                    url = HostPath + url.TrimStart('/');

                if (url.StartsWith("http") == false)
                    url = "https://" + url.TrimStart('/');
            }
            return url;
        }

        public static bool IsCollection(Type t)
        {
            return t.GetInterfaces().Any(iface => iface.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        public static int GetListItemIndex(ListControl control, ListItem item)
        {
            int index = control.Items.IndexOf(item);
            if (index == -1)
                throw new NullReferenceException("ListItem does not exist ListControl.");

            return index;
        }

        public static string GetPhysicalPath(string folderName, string fileName)
        {
            return string.Format("{0}{1}\\{2}", HttpContext.Current.Request.PhysicalApplicationPath, folderName, fileName);
        }

        public static bool IsValidWwwPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            Regex r = new Regex(@"(http|https|ftp)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            return r.IsMatch(path);
        }

        public static string HostPath
        {
            get
            {
                return CommonHelpers.GetSiteRoot() + "/"; ;
            }
        }

        public static string GetServerIP()
        {
            string myHost = Dns.GetHostName();
            string myIP = Dns.GetHostEntry(myHost).AddressList[0].ToString();
            return myIP;
        }

        static string ReplaceCapText(Match ma)
        {
            string ss = ma.Groups[1].Value.Trim();
            if (ss.StartsWith("\""))
                return "\"/uploads";
            else if (ss.StartsWith("'"))
                return "'/uploads";
            else
                return "/uploads";
        }

        public static string ConvertToSavePath(string content, bool isPath)
        {

            Regex re = null;
            if (isPath == false)
                re = new Regex(@"([""|']+https?:\/\/\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b|[""|']sv([\d]+)?|[""'])\/uploads", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            else
                re = new Regex(@"(https?:\/\/\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b|sv([\d]+)?|)\/uploads", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Match ma = re.Match(content);

            while (ma.Success == true)
            {
                content = re.Replace(content, new MatchEvaluator(ReplaceCapText));

                ma = ma.NextMatch();
            }

            return content;
        }

        public static string RemoveHtml(string text)
        {
            try
            {
                return Regex.Replace(text, "<[^>]*>", string.Empty);
            }
            catch
            {
                return text;
            }
        }

        public static string CreateSlugUrl(string phrase)
        {
            //First to lower case 
            phrase = VnUnicodeHelpers.ReplaceVietnameseCharacters(phrase).ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(phrase);

            phrase = Encoding.ASCII.GetString(bytes);

            //Replace spaces 
            phrase = Regex.Replace(phrase, @"\s", "-", RegexOptions.Compiled);

            //Remove invalid chars 
            phrase = Regex.Replace(phrase, @"[^\w\s\p{Pd}]", "", RegexOptions.Compiled);

            //Trim dashes from end 
            phrase = phrase.Trim('-', '_');

            //Replace double occurences of - or \_ 
            phrase = Regex.Replace(phrase, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return phrase;
        }
        public static string NormalizeFileName(string phrase)
        {
            //First to lower case 
            phrase = VnUnicodeHelpers.ReplaceVietnameseCharacters(phrase).ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(phrase);

            phrase = Encoding.ASCII.GetString(bytes);

            //Replace spaces 
            phrase = Regex.Replace(phrase, @"\s", "_", RegexOptions.Compiled);

            //Remove invalid chars 
            phrase = Regex.Replace(phrase, @"[^\w\s\p{Pd}]", "", RegexOptions.Compiled);

            //Trim dashes from end 
            phrase = phrase.Trim('-', '_');

            //Replace double occurences of - or \_ 
            phrase = Regex.Replace(phrase, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return phrase;
        }
        public static string IpClientAddress()
        {
            HttpRequest _request = HttpContext.Current.Request;
            try
            {
                if (!string.IsNullOrEmpty(_request.ServerVariables["HTTP_X_REAL_IP"]))
                    return _request.ServerVariables["HTTP_X_REAL_IP"];
                else if (!string.IsNullOrEmpty(_request.ServerVariables["HTTP_CLIENT_IP"]))
                    return _request.ServerVariables["HTTP_CLIENT_IP"];
                else if (!string.IsNullOrEmpty(_request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
                    return _request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                else
                    return _request.ServerVariables["REMOTE_ADDR"];
            }
            catch
            {
                if (_request != null)
                    return _request.ServerVariables["REMOTE_ADDR"];
                return string.Empty;
            }
        }

        public static string GetDescription<T>(string fieldName)
        {
            string result;
            FieldInfo fi = typeof(T).GetField(fieldName.ToString());
            if (fi != null)
            {
                try
                {
                    object[] descriptionAttrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
                    result = (description.Description);
                }
                catch
                {
                    result = string.Empty;
                }
            }
            else
            {
                result = string.Empty;
            }

            return result;
        }

        public static string GetCategory<T>(string fieldName)
        {
            string result;
            FieldInfo fi = typeof(T).GetField(fieldName.ToString());
            if (fi != null)
            {
                try
                {
                    object[] categoryAttrs = fi.GetCustomAttributes(typeof(CategoryAttribute), false);
                    CategoryAttribute category = (CategoryAttribute)categoryAttrs[0];
                    result = (category.Category);
                }
                catch
                {
                    result = string.Empty;
                }
            }
            else
            {
                result = string.Empty;
            }

            return result;
        }

        public static string CheckHasChecked(object value)
        {
            try
            {
                if (value == null)
                    return string.Empty;
                bool bValue = false;
                bool.TryParse(value.ToString(), out bValue);
                if (bValue)
                    return "<i class=\"fa fa-check-circle lable-success\" aria-hidden=\"true\"></i>";
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static Dictionary<CMSImageType, string> GetListImageDimension()
        {
            Dictionary<CMSImageType, string> dic = new Dictionary<CMSImageType, string>();
            Array values = Enum.GetValues(typeof(CMSImageType));
            if (values != null && values.Length > 0)
            {
                RenderAttribute type = null;
                foreach (var val in values)
                {
                    type = Helpers.GetRenderAttribute((CMSImageType)val);
                    if (type != null)
                    {
                        if (type.Width > 0 && type.Height > 0)
                            dic.Add((CMSImageType)val, type.Width + "px x " + type.Height + "px");
                    }
                }
            }
            return dic;
        }
        public static RenderAttribute GetRenderAttribute(CMSImageType imageType)
        {
            Type type = imageType.GetType();
            try
            {
                System.Reflection.FieldInfo fieldInfo = type.GetField(imageType.ToString());
                RenderAttribute attribute = fieldInfo.GetCustomAttributes(typeof(RenderAttribute), false).FirstOrDefault() as RenderAttribute;
                if (attribute == null)
                    return null;

                return attribute;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string GetAvatar(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/assets/images/no-avatar.jpg";
            return path;
        }
        public static string GetThumbnailUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/assets/images/no-image.jpg";
            return $"{HostPath.TrimEnd('/')}/{path.TrimStart('/')}";
        }
        #region File size
        public static class FileSizeFormatter
        {
            // Load all suffixes in an array  
            static readonly string[] suffixes =
            { "Bytes", "KB", "MB", "GB", "TB", "PB" };
            public static string FormatSize(Int64 bytes)
            {
                int counter = 0;
                decimal number = (decimal)bytes;
                while (Math.Round(number / 1024) >= 1)
                {
                    number = number / 1024;
                    counter++;
                }
                return string.Format("{0:n1}{1}", number, suffixes[counter]);
            }
            public static string GetFileSize(string fileName)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    string path = HttpContext.Current.Server.MapPath(fileName);
                    if (!string.IsNullOrEmpty(path))
                    {
                        FileInfo fi = new FileInfo(path);
                        if (fi.Exists)
                        {
                            string size = FileSizeFormatter.FormatSize(fi.Length);
                            return size;
                        }
                    }
                }
                return string.Empty;
            }
            public static string GetFileName(string fileName)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    string path = HttpContext.Current.Server.MapPath(fileName);
                    if (!string.IsNullOrEmpty(path))
                    {
                        FileInfo fi = new FileInfo(path);
                        if (fi.Exists)
                        {
                            return fi.Name;
                        }
                    }
                }
                return string.Empty;
            }
        }
        #endregion
        public static string ConvertHtmlInnerToClientString(object htmlInner)
        {
            return ConvertHtmlInnerToClientString(htmlInner.ToString());
        }
        public static string ConvertHtmlInnerToClientString(string htmlInner)
        {
            return htmlInner.Trim().Replace("\r\n", "\\").Replace("\n", "\\").Replace("'", "&#39;").Replace("\"", "\\\"");
        }
        public static string MaskBankAccountNumber(string _number)
        {
            return _number;
            int lengthToDisplay = 4; // Number of digits to display
            int totalLength = _number.Length;

            // Make sure the total length is greater than or equal to the length to display
            if (totalLength < lengthToDisplay)
            {
                return _number; // Return the original BankAccount number if it's too short
            }

            // Get the last 'lengthToDisplay' characters
            string lastDigits = _number.Substring(totalLength - lengthToDisplay, lengthToDisplay);

            // Create a mask with asterisks (*) of the same length
            string mask = new string('*', lengthToDisplay);

            // Combine the masked part and the last digits
            return mask + lastDigits;
        }
        public static Dictionary<string, object> ConvertSubsonicTableObj(Type columType, Type tableType, object item)
        {
            if (item == null)
                return null;

            Dictionary<string, object> convertedItem = new Dictionary<string, object>();

            System.Reflection.PropertyInfo propertyInfo;
            foreach (System.Reflection.FieldInfo fieldInfo in columType.GetFields())
            {
                propertyInfo = tableType.GetProperty(fieldInfo.Name);
                var hValue = propertyInfo.GetValue(item, null);

                convertedItem.Add(fieldInfo.Name, hValue);
            }
            return convertedItem;
        }
        public static bool IsValidType<T>(string type)
        {
            Type objType = typeof(T);
            FieldInfo[] info = objType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (info == null)
                return false;

            foreach (FieldInfo fieldInfo in info)
            {
                if (type == fieldInfo.Name)
                    return true;
            }
            return false;
        }
        public static bool IsValidEnumValue<T>(string value) where T : struct, Enum
        {
            return Enum.TryParse<T>(value, out _);
        }
        public static TTarget CopyProperties<TSource, TTarget>(TSource source) where TTarget : TSource, new()
        {
            var target = new TTarget();
            var sourceProperties = typeof(TSource).GetProperties();
            var targetProperties = typeof(TTarget).GetProperties();

            foreach (var sourceProp in sourceProperties)
            {
                var targetProp = targetProperties.FirstOrDefault(p => p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
                if (targetProp != null && targetProp.CanWrite)
                {
                    targetProp.SetValue(target, sourceProp.GetValue(source));
                }
            }

            return target;
        }
    }
}
