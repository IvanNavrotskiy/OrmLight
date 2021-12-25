using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> AddCondition<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            return source.Where(predicate);
        }

        public static IQueryable<TSource> AddSort<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector,
            bool isDescending = false)
        {
            return isDescending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
        }

        public static IQueryable<TSource> AddSortByDescending<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.AddSort(keySelector, isDescending: true);
        }

        public static IQueryable<TSource> AddLimit<TSource>(this IQueryable<TSource> source, int count, int offset)
        {           
            return source.Take(count).Skip(offset);
        }

        public static IQueryable<TSource> AddOffset<TSource>(this IQueryable<TSource> source, int offset)
        {
            return source.Skip(offset);
        }

        public static IEnumerable<T> Execute<T>(this IQueryable<T> source)
        {
            return source.ToList();
        }
    }
}
