using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SweetSoft.QLDA.Core.Caches
{
    public class CacheManager
    {
        public class CacheKeys
        {
            public const string DASHBOARD_REPORT = "DASHBOARD_REPORT";
            public const string PHYSICAL_GOLD_LIST = "PHYSICAL_GOLD_LIST";
            public const string DIGITAL_GOLD_LIST = "DIGITAL_GOLD_LIST";
            public const string PHYSICAL_SELL_LIST = "PHYSICAL_SELL_LIST";
            public const string DIGITAL_SELL_LIST = "DIGITAL_SELL_LIST";
            public const string PHYSICAL_CONVERT_LIST = "PHYSICAL_CONVERT_LIST";
            public const string DIGITAL_CONVERT_LIST = "DIGITAL_CONVERT_LIST";
            public const string APPOINTMENT_LIST = "APPOINTMENT_LIST";
            public const string GOLD_SETTING_LIST = "GOLD_SETTING_LIST";
            public const string TOP_UP_LIST = "TOP_UP_LIST";
            public const string WITHDRAW_LIST = "WITHDRAW_LIST";
        }
        #region ez cache
        public class ObjCache
        {
            public DateTime UpdatedTime { get; set; }
            public object Data { get; set; }
        }
        private static HashSet<string> CachedKeys = new HashSet<string>();
        public static bool GetCacheData<T>(string key, out T resultData)
        {
            CacheManager.ObjCache cacheValue = AppCache.Get(key) as CacheManager.ObjCache;
            if (cacheValue != null)
            {
                resultData = (T)cacheValue.Data;
                if (resultData != null)
                    return true;
                resultData = default(T);
                return false;
            }
            resultData = default(T);
            return false;
        }
        public static bool GetCacheData<T>(string key, out T resultData, out DateTime lastTime)
        {
            lastTime = DateTime.UtcNow;
            CacheManager.ObjCache cacheValue = AppCache.Get(key) as CacheManager.ObjCache;
            if (cacheValue != null)
            {
                lastTime = cacheValue.UpdatedTime;
                resultData = (T)cacheValue.Data;
                if (resultData != null)
                    return true;
                resultData = default(T);
                return false;
            }
            resultData = default(T);
            return false;
        }
        public static void SetCacheData(string key, object data)
        {
            ObjCache cacheValue = new ObjCache();
            cacheValue.UpdatedTime = DateTime.UtcNow;
            cacheValue.Data = data;

            AppCache.Insert(key, cacheValue, 10 * 60);
            lock (CachedKeys)
            {
                CachedKeys.Add(key);
            }
        }
        public static bool GetCacheData<T>(string key, object[] parameters, out T resultData, out DateTime lastTime)
        {
            key = GetKeyByParameters(key, parameters);
            return GetCacheData(key, out resultData, out lastTime);
        }
        public static void SetCacheData(string key, object[] parameters, object data)
        {
            key = GetKeyByParameters(key, parameters);
            SetCacheData(key, data);
        }
        public static void ClearCache(string key, object[] parameters)
        {
            key = GetKeyByParameters(key, parameters);
            AppCache.Remove(key);
        }
        public static void ClearCache(string key)
        {
            lock (CachedKeys)
            {
                var keysToRemove = CachedKeys.Where(k => k.StartsWith(key + "|")).ToList();
                foreach (var cacheKey in keysToRemove)
                {
                    AppCache.Remove(cacheKey);
                    CachedKeys.Remove(cacheKey);
                }
            }
        }
        private static string GetKeyByParameters(string key, object[] parameters)
        {
            string cacheKey = key + '|';
            cacheKey += JsonConvert.SerializeObject(parameters);
            return cacheKey;
        }
        #endregion
    }
}
