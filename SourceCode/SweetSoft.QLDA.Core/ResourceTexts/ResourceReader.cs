//----------------------PROGRAMER LOGS------------------------
//'**Change 01: Truong, 29 Oct 2024 - Fix bug
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace SweetSoft.QLDA.Core.ResourceTexts
{
    public abstract class ResourceReader
    {
        private static ConcurrentDictionary<string, ResourceManager> resourceManagers = new ConcurrentDictionary<string, ResourceManager>();

        public static void RenewResourceManager()
        {
            resourceManagers.Clear();
        }

        public static string GetResourceText(string messageId, Type invokingType, string baseName, params object[] args)
        {
            return GetResourceText(CultureInfo.CurrentUICulture, messageId, invokingType, baseName, args);
        }

        public static string GetResourceText(CultureInfo cultureInfo, string messageId, Type invokingType, string baseName, params object[] args)
        {
            string nameSpace = invokingType.Namespace ?? string.Empty;
            string fullBaseName = $"{nameSpace.Trim('.')}.{baseName.Trim('.')}";
            string key = $"{fullBaseName}:{invokingType.Assembly}";

            ResourceManager rm = GetOrAddResourceManager(fullBaseName, invokingType.Assembly, key);

            string message = TryGetString(rm, messageId, cultureInfo);

            if (string.IsNullOrEmpty(message) || message == messageId)
            {
                // Force renew in case the original ResourceManager is outdated
                rm = new ResourceManager(fullBaseName, invokingType.Assembly);
                resourceManagers[key] = rm;
                message = TryGetString(rm, messageId, cultureInfo);
            }

            if (!string.IsNullOrEmpty(message) && args?.Length > 0)
            {
                message = string.Format(cultureInfo, message, args);
            }

            return message;
        }

        private static string TryGetString(ResourceManager rm, string messageId, CultureInfo culture)
        {
            try
            {
                return rm.GetString(messageId, culture) ?? messageId;
            }
            catch
            {
                return messageId;
            }
        }

        private static ResourceManager GetOrAddResourceManager(string baseName, Assembly assembly, string key)
        {
            return resourceManagers.GetOrAdd(key, _ => new ResourceManager(baseName, assembly));
        }
    }
}
