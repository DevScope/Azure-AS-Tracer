using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;
using System.Runtime.Caching;

namespace DevScope.Framework.Common.Utils
{
    public static class CacheHelper
    {
        //public const string CachePrefix = "CacheHelper.";
        static readonly object locker = new object();

        private static MemoryCache cacheEngine;
        private static MemoryCache CacheEngine
        {
            get
            {
                if (cacheEngine != null)
                {
                    return cacheEngine;
                }

                cacheEngine = new MemoryCache("CacheHelper");

                return cacheEngine;
            }
        }

        public static bool Contains(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            return CacheEngine.Contains(key);
        }

        public static object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            object obj = null;

            lock (locker)
            {
                obj = CacheEngine[BuildCacheKey(key)];
            }

            if (obj == null)
            {
                return null;
            }

            return obj;
        }

        public static bool Set(string key, object obj, int minutesToExpire = 30)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            var cacheKey = BuildCacheKey(key);

            var policy = new CacheItemPolicy();

            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minutesToExpire);

            lock (locker)
            {
                return CacheEngine.Add(cacheKey, obj, policy);
            }
        }

        private static string BuildCacheKey(string key)
        {
            //return string.Format("{0}{1}", CachePrefix, key);
            return key;
        }

        public static void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");            

            lock (locker)
            {
                CacheEngine.Remove(key);
            }
        }

        public static void ResetCache(string startsWith = null)
        {
            var startsWithMatch = BuildCacheKey(startsWith);

            foreach (var cacheItem in CacheEngine)
            {                
                if (string.IsNullOrEmpty(startsWith) 
                    || (cacheItem.Key.StartsWith(startsWithMatch)))
                {
                    CacheEngine.Remove(cacheItem.Key);
                }
            }
        }
    }
}
