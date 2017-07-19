using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DevScope.Framework.Common.Extensions
{
    public static class Linq
    {
        public static Dictionary<TKey, TData> CreateIndex<TSource, TKey, TData>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TData> dataSelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            if (dataSelector == null)
                throw new ArgumentNullException("dataSelector");

            var index = new Dictionary<TKey, TData>();

            foreach (var item in source)
            {
                var key = keySelector(item);
                var data = dataSelector(item);

                index.Add(key, data);                
            }

            return index;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            var knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            
            List<T> nextbatch = new List<T>(batchSize);

            foreach (T item in collection)
            {
                nextbatch.Add(item);
                if (nextbatch.Count == batchSize)
                {
                    yield return nextbatch;
                    nextbatch = new List<T>(batchSize);
                }
            }

            if (nextbatch.Count > 0)
            {
                yield return nextbatch;
            }
        }
    }
}
