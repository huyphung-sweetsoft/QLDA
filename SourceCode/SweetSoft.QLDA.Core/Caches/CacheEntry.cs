using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Caches
{
    internal class CacheEntry
    {
        private TCache.CacheLoaderDelegate _CacheLoader;
        private DateTime _CreateDateTime = DateTime.UtcNow;
        private object _InternalLocker;
        private DateTime _LastUpdate = DateTime.MaxValue;
        private DateTime _LastUse = DateTime.MinValue;
        private readonly object _Locker = new object();
        private TimeSpan _RefreshInterval;
        private TimeSpan _SlidingExpiration;

        internal CacheEntry()
        {
        }

        public TimeSpan Age
        {
            get
            {
                return (TimeSpan)(this._LastUse - this._LastUpdate);
            }
        }

        public TCache.CacheLoaderDelegate CacheLoader
        {
            get
            {
                return this._CacheLoader;
            }
            set
            {
                this._CacheLoader = value;
            }
        }

        public DateTime CreateDateTime
        {
            get
            {
                return this._CreateDateTime;
            }
        }

        public object InternalLocker
        {
            get
            {
                if (this._InternalLocker == null)
                {
                    Interlocked.CompareExchange(ref this._InternalLocker, new object(), null);
                }
                return this._InternalLocker;
            }
        }

        public DateTime LastUpdate
        {
            get
            {
                return this._LastUpdate;
            }
            set
            {
                this._LastUpdate = value;
            }
        }

        public DateTime LastUse
        {
            get
            {
                return this._LastUse;
            }
            set
            {
                this._LastUse = value;
            }
        }

        public object Locker
        {
            get
            {
                return this._Locker;
            }
        }

        public TimeSpan RefreshInterval
        {
            get
            {
                return this._RefreshInterval;
            }
            set
            {
                this._RefreshInterval = value;
            }
        }

        public TimeSpan SlidingExpiration
        {
            get
            {
                return this._SlidingExpiration;
            }
            set
            {
                this._SlidingExpiration = value;
            }
        }
    }
}
