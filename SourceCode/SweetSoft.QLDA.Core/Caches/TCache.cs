using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;

namespace SweetSoft.QLDA.Core.Caches
{
    public class TCache
    {
        private static readonly Cache _cache = HttpRuntime.Cache;
        private static CacheLoaderErrorDelegate _cacheLoaderErrorDelegate = null;
        public static readonly int DayFactor = 0x4380;
        private static int Factor = 5;
        public static readonly int HourFactor = 720;
        public static readonly int MinuteFactor = 12;
        public static readonly double SecondFactor = 0.2;

        private TCache()
        {
        }

        public static void Clear()
        {
            IDictionaryEnumerator enumerator = _cache.GetEnumerator();
            ArrayList list = new ArrayList();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Key);
            }
            foreach (string str in list)
            {
                _cache.Remove(str);
            }
        }

        public static bool ContainsCacheEntry(string key)
        {
            return CacheLockbox.ContainsCacheEntry(key);
        }

        public static object Get(string key)
        {
            return _cache[key];
        }

        public static object GetCacheEntryLock(string key)
        {
            return CacheLockbox.GetLock(key);
        }

        public static object GetCacheEntryLock(string key, int refreshIntervalSeconds, int slidingExpirationSeconds, CacheLoaderDelegate loaderDelegate)
        {
            return CacheLockbox.GetLock(key, TimeSpan.FromSeconds((double)(refreshIntervalSeconds * Factor)), TimeSpan.FromSeconds((double)(slidingExpirationSeconds * Factor)), loaderDelegate);
        }

        public static object GetCacheEntryLock(string key, TimeSpan refreshInterval, TimeSpan slidingExpiration, CacheLoaderDelegate loaderDelegate)
        {
            return CacheLockbox.GetLock(key, refreshInterval, slidingExpiration, loaderDelegate);
        }

        public static void Insert(string key, object obj)
        {
            Insert(key, obj, null, 1);
        }

        public static void Insert(string key, object obj, int seconds)
        {
            Insert(key, obj, null, seconds);
        }

        public static void Insert(string key, object obj, CacheDependency dep)
        {
            Insert(key, obj, dep, MinuteFactor * 3);
        }

        public static void Insert(string key, object obj, int seconds, CacheItemPriority priority)
        {
            Insert(key, obj, null, seconds, priority);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds)
        {
            Insert(key, obj, dep, seconds, CacheItemPriority.Normal);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority)
        {
            Insert(key, obj, dep, seconds, priority, null);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, dep, DateTime.UtcNow.AddSeconds((double)(Factor * seconds)), TimeSpan.Zero, priority, onRemoveCallback);
            }
        }

        internal static object InternalCallback(string key)
        {
            CacheEntry cacheEntry = CacheLockbox.GetCacheEntry(key);
            if (cacheEntry == null)
            {
                return null;
            }
            CacheLoaderDelegate cacheLoader = cacheEntry.CacheLoader;
            if (cacheLoader == null)
            {
                return null;
            }
            object obj2 = null;
            try
            {
                obj2 = cacheLoader();
            }
            catch (Exception exception)
            {
                if (_cacheLoaderErrorDelegate != null)
                {
                    try
                    {
                        _cacheLoaderErrorDelegate(key, exception);
                    }
                    catch
                    {
                    }
                }
            }
            if (obj2 != null)
            {
                Insert(key, obj2, null, (int)cacheEntry.RefreshInterval.TotalSeconds, CacheItemPriority.Normal, new CacheItemRemovedCallback(TCache.ItemRemovedCallback));
                CacheLockbox.UpdateCacheEntry(key, DateTime.UtcNow);
            }
            return obj2;
        }

        internal static void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            if (reason == CacheItemRemovedReason.Expired)
            {
                CacheEntry cacheEntry = CacheLockbox.GetCacheEntry(key);
                if (cacheEntry.LastUse.Add(cacheEntry.SlidingExpiration) > DateTime.UtcNow)
                {
                    _cache.Insert(key, value, null, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30.0)), TimeSpan.Zero, CacheItemPriority.Low, null);
                    ThreadPool.QueueUserWorkItem(delegate (object o)
                    {
                        string _key = o.ToString();
                        lock (CacheLockbox.GetInternalLock(_key))
                        {
                            InternalCallback(_key);
                        }
                    }, key);
                }
            }
        }

        public static void Max(string key, object obj)
        {
            Max(key, obj, null);
        }

        public static void Max(string key, object obj, CacheDependency dep)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, dep, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.AboveNormal, null);
            }
        }

        public static void MicroInsert(string key, object obj, int secondFactor)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, null, DateTime.UtcNow.AddSeconds((double)(Factor * secondFactor)), TimeSpan.Zero);
            }
        }

        public static void Permanent(string key, object obj)
        {
            Permanent(key, obj, null);
        }

        public static void Permanent(string key, object obj, CacheDependency dep)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, dep, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
            }
        }

        public static bool RefreshByCacheKey(string key)
        {
            if (!CacheLockbox.ContainsCacheEntry(key))
            {
                return false;
            }
            ThreadPool.QueueUserWorkItem(delegate (object o)
            {
                Update(o.ToString());
            }, key);
            return true;
        }

        public static void RefreshByPattern(string pattern)
        {
            ThreadPool.QueueUserWorkItem(delegate (object _pattern)
            {
                IEnumerator<string> enumerator = CacheLockbox.Keys.GetEnumerator();
                Regex regex = new Regex(_pattern.ToString(), RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
                while (enumerator.MoveNext())
                {
                    string current = enumerator.Current;
                    if (regex.IsMatch(current))
                    {
                        lock (CacheLockbox.GetInternalLock(current))
                        {
                            InternalCallback(current);
                            continue;
                        }
                    }
                }
            }, pattern);
        }

        public static void Remove(string key)
        {
            _cache.Remove(key);
        }

        public static void RemoveByPattern(string pattern)
        {
            IDictionaryEnumerator enumerator = _cache.GetEnumerator();
            Regex regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            while (enumerator.MoveNext())
            {
                if (regex.IsMatch(enumerator.Key.ToString()))
                {
                    _cache.Remove(enumerator.Key.ToString());
                }
            }
        }

        public static void ReSetFactor(int cacheFactor)
        {
            Factor = cacheFactor;
        }

        public static int SecondFactorCalculate(int seconds)
        {
            return Convert.ToInt32(Math.Round((double)(seconds * SecondFactor), 2));
        }

        public static void SetCacheLoaderErrorHandler(CacheLoaderErrorDelegate handler)
        {
            if (_cacheLoaderErrorDelegate != null)
            {
                throw new Exception("The CacheLoaderDelegate should only be set once within an app.");
            }
            if (handler != null)
            {
                _cacheLoaderErrorDelegate = handler;
            }
        }

        public static bool Update(string key)
        {
            if (!CacheLockbox.ContainsCacheEntry(key))
            {
                return false;
            }
            lock (CacheLockbox.GetInternalLock(key))
            {
                InternalCallback(key);
            }
            return true;
        }

        public static int CacheFactor
        {
            get
            {
                return Factor;
            }
        }

        public delegate object CacheLoaderDelegate();

        public delegate void CacheLoaderErrorDelegate(string cacheKey, Exception e);
    }

    public class TCache<T> where T : class
    {
        private TCache()
        {
        }

        public static T Get(string key)
        {
            if (CacheLockbox.ContainsCacheEntry(key))
            {
                lock (CacheLockbox.GetLock(key))
                {
                    T local = TCache.Get(key) as T;
                    if (local == null)
                    {
                        local = TCache.InternalCallback(key) as T;
                    }
                    return local;
                }
            }
            throw new Exception("CacheEntry for key '" + key + "' doesn't exist.  Please call a different overload of TCache<T>.Get() to set the CacheEntry properties.");
        }

        public static T Get(string key, int refreshIntervalSeconds, int slidingExpirationSeconds, TCache.CacheLoaderDelegate loaderDelegate)
        {
            return TCache<T>.Get(key, TimeSpan.FromSeconds((double)(refreshIntervalSeconds * TCache.CacheFactor)), TimeSpan.FromSeconds((double)(slidingExpirationSeconds * TCache.CacheFactor)), loaderDelegate);
        }

        public static T Get(string key, TimeSpan refreshInterval, TimeSpan slidingExpiration, TCache.CacheLoaderDelegate loaderDelegate)
        {
            lock (CacheLockbox.GetLock(key, refreshInterval, slidingExpiration, loaderDelegate))
            {
                T local = TCache.Get(key) as T;
                if (local == null)
                {
                    local = TCache.InternalCallback(key) as T;
                }
                return local;
            }
        }

        public static bool Update(string key)
        {
            return TCache.Update(key);
        }
    }
}
