//-----------------------PROGRAMER LOGS---------------------------

using SweetSoft.QLDA.Core.Managers;
using System;
using System.Web.Caching;
namespace SweetSoft.QLDA.Core.Caches
{
    public class AppCache
    {
        #region Constructors

        private AppCache() { }

        #endregion

        //>> Based on Factor = 5 default value
        public static readonly int DayFactor = TCache.DayFactor;
        public static readonly int HourFactor = TCache.HourFactor;
        public static readonly int MinuteFactor = TCache.MinuteFactor;
        public static readonly double SecondFactor = TCache.SecondFactor;

        #region public static int CacheFactor
        //'**Change 01: start
        public static string GetCacheKey(string functionNamespace, string functionName, object[] functionParameter)
        {
            string key = $"{functionNamespace}.{functionName}";
            foreach (object parameter in functionParameter)
            {
                key += $".{(parameter == null ? "" : parameter.ToString())}";
            }
            return key;
        }
        //'**Change 01: end

        public static int CacheFactor
        {
            get { return TCache.CacheFactor; }
        }

        #endregion

        #region public static void ReSetFactor(int cacheFactor)

        public static void ReSetFactor(int cacheFactor)
        {
            TCache.ReSetFactor(cacheFactor);
        }

        #endregion

        #region public static void Clear()

        /// <summary>
        /// Removes all items from the Cache
        /// </summary>
        public static void Clear()
        {
            TCache.Clear();
        }

        #endregion

        #region public static void RemoveByPattern(string pattern)

        public static void RemoveByPattern(string pattern)
        {
            TCache.RemoveByPattern(pattern);
        }

        #endregion

        #region public static void Remove(string key)

        /// <summary>
        /// Removes the specified key from the cache
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            TCache.Remove(key);
        }

        #endregion

        #region public static void Insert(...) overloads

        /// <summary>
        /// Insert the current "obj" into the cache. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Insert(string key, object obj)
        {
            TCache.Insert(key, obj, 1800); //By default, cache in 30 minutes
        }

        public static void Insert(string key, object obj, CacheDependency dep)
        {
            TCache.Insert(key, obj, dep);
        }

        public static void Insert(string key, object obj, int seconds)
        {
            TCache.Insert(key, obj, seconds);
        }

        public static void Insert(string key, object obj, int seconds, CacheItemPriority priority)
        {
            TCache.Insert(key, obj, seconds, priority);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds)
        {
            TCache.Insert(key, obj, dep, seconds);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority)
        {
            TCache.Insert(key, obj, dep, seconds, priority);
        }

        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            TCache.Insert(key, obj, dep, seconds, priority, onRemoveCallback);
        }

        #endregion

        #region public static void MicroInsert (string key, object obj, int secondFactor)

        public static void MicroInsert(string key, object obj, int secondFactor)
        {
            TCache.MicroInsert(key, obj, secondFactor);
        }

        #endregion

        #region public static void Max(...) overloads

        /// <summary>
        /// Insert an item into the cache for the Maximum allowed time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Max(string key, object obj)
        {
            TCache.Max(key, obj);
        }

        public static void Max(string key, object obj, CacheDependency dep)
        {
            TCache.Max(key, obj, dep);
        }

        #endregion

        #region public static void Permanent(...) overloads

        /// <summary>
        /// Insert an item into the cache for the Maximum allowed time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Permanent(string key, object obj)
        {
            TCache.Permanent(key, obj);
        }

        public static void Permanent(string key, object obj, CacheDependency dep)
        {
            TCache.Permanent(key, obj, dep);
        }

        #endregion

        #region public static object Get(string key)

        public static object Get(string key)
        {
            return TCache.Get(key);
        }

        #endregion

        #region public static int SecondFactorCalculate(int seconds)

        /// <summary>
        /// Return int of seconds * SecondFactor
        /// </summary>
        public static int SecondFactorCalculate(int seconds)
        {
            // Insert method below takes integer seconds, so we have to round any fractional values
            return TCache.SecondFactorCalculate(seconds);
        }

        #endregion

        #region public static void RefreshByPattern(string pattern)

        public static void RefreshByPattern(string pattern)
        {
            TCache.RefreshByPattern(pattern);
        }

        #endregion

        #region public static bool RefreshByCacheKey(string key)

        public static bool RefreshByCacheKey(string key)
        {
            return TCache.RefreshByCacheKey(key);
        }

        #endregion

        #region public static bool Update(string key)

        public static bool Update(string key)
        {
            return TCache.Update(key);
        }

        #endregion

        //#region public static void SetCacheLoaderErrorHandler(TCache.CacheLoaderErrorDelegate handler)

        //public static void SetCacheLoaderErrorHandler(TCache.CacheLoaderErrorDelegate handler)
        //{
        //    TCache.SetCacheLoaderErrorHandler( handler );
        //}

        //#endregion

        #region public static object GetCacheEntryLock(...) overloads

        public static object GetCacheEntryLock(string key)
        {
            return TCache.GetCacheEntryLock(key);
        }

        #endregion

        #region public static bool ContainsCacheEntry(stirng key)

        public static bool ContainsCacheEntry(string key)
        {
            return TCache.ContainsCacheEntry(key);
        }

        #endregion
    }
}
