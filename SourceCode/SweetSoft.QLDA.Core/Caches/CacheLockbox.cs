using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Caches
{
    internal class CacheLockbox
    {
        private static readonly Dictionary<string, CacheEntry> _Lockbox = new Dictionary<string, CacheEntry>();
        private static readonly object _ReadLockbox = new object();
        private static readonly object _WriteLockbox = new object();

        private CacheLockbox()
        {
        }

        public static bool ContainsCacheEntry(string key)
        {
            return Instance.ContainsKey(key);
        }

        public static CacheEntry GetCacheEntry(string key)
        {
            lock (_ReadLockbox)
            {
                if (ContainsCacheEntry(key))
                {
                    return Instance[key];
                }
                return null;
            }
        }

        public static object GetInternalLock(string key)
        {
            lock (_ReadLockbox)
            {
                CacheEntry entry = null;
                if (ContainsCacheEntry(key))
                {
                    entry = Instance[key];
                }
                if (entry != null)
                {
                    return entry.InternalLocker;
                }
                return null;
            }
        }

        public static object GetLock(string key)
        {
            lock (_ReadLockbox)
            {
                CacheEntry entry = null;
                if (ContainsCacheEntry(key))
                {
                    entry = Instance[key];
                }
                if (entry != null)
                {
                    entry.LastUse = DateTime.UtcNow;
                    return entry.Locker;
                }
                return new Exception(string.Format("Cacheentry for key '{0}' doesn't exist.  Please call GetLock(string key, TimeSpan refreshInterval, TimeSpan refreshDuration, TCache.CacheLoaderDelegate cacheLoader) to set a CacheEntry.", key));
            }
        }

        public static object GetLock(string key, TimeSpan refreshInterval, TimeSpan slidingExpiration, TCache.CacheLoaderDelegate cacheLoader)
        {
            lock (_ReadLockbox)
            {
                CacheEntry entry;
                if (ContainsCacheEntry(key))
                {
                    entry = Instance[key];
                    entry.LastUse = DateTime.UtcNow;
                }
                else
                {
                    lock (_WriteLockbox)
                    {
                        entry = new CacheEntry();
                        entry.CacheLoader = cacheLoader;
                        entry.RefreshInterval = refreshInterval;
                        entry.SlidingExpiration = slidingExpiration;
                        entry.LastUse = DateTime.UtcNow;
                        Instance.Add(key, entry);
                    }
                }
                return entry.Locker;
            }
        }

        public static void UpdateCacheEntry(string key, DateTime lastUpdate)
        {
            lock (_WriteLockbox)
            {
                if (ContainsCacheEntry(key))
                {
                    CacheEntry entry = Instance[key];
                    entry.LastUpdate = lastUpdate;
                }
            }
        }

        private static Dictionary<string, CacheEntry> Instance
        {
            get
            {
                return _Lockbox;
            }
        }

        public static Dictionary<string, CacheEntry>.KeyCollection Keys
        {
            get
            {
                return Instance.Keys;
            }
        }
    }
}
