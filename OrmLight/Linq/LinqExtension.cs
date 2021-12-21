using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight.Linq
{
    public static class LinqExtension
    {
        public static IEnumerable<T> AddCondition<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            return collection.Where(predicate);
        }

        public static IEnumerable<T> Execute<T>(this IEnumerable<T> collection)
        {
            return collection.ToList();
        }

        public static IEnumerable<TSource> AddSort<TSource, TKey>(this IEnumerable<TSource> collection, Func<TSource, TKey> keySelector, bool isDescending = false)
        {
            if (isDescending)
                return collection.OrderByDescending(keySelector);

            return collection.OrderBy(keySelector);
        }
    }
}
