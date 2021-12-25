using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight.Linq.Expressions
{
    public static class LinqExtension
    {
        private static IQueryProvider Provider<TSource>(IEnumerable<TSource> collection)
        {
            return (collection as QueryableSource<TSource>).Provider;
        }

        //TODO:Change signature
        public static IEnumerable<T> AddCondition<T>(this IEnumerable<T> collection, Expression<Func<T, bool>> predicate)
        {
            var method = typeof(Enumerable).GetMethods().Where(m => m.Name.Contains("Where")).FirstOrDefault().MakeGenericMethod(typeof(T)); //TODO:->GetMethod
            var expr = Expression.Call(method, Expression.Constant(collection), predicate);
            var provider = (collection as QueryableSource<T>).Provider;
            Provider(collection)?.CreateQuery(expr);

            return collection;
        }

        public static IEnumerable<T> Execute<T>(this IEnumerable<T> collection)
        {
            //var source = (QueryableSource<T>)collection;
            //var claims = (source.Provider as QueryProvider<T>).Claims;
            //claims = claims.ToList();

            return collection.ToList();
        }

        public static IEnumerable<TSource> AddSortByDescending<TSource, TKey>(this IEnumerable<TSource> collection, Expression<Func<TSource, TKey>> keySelector)
        {
            return collection.AddSort(keySelector, isDescending: true);
        }


        public static IEnumerable<TSource> AddSort<TSource, TKey>(this IEnumerable<TSource> collection, Expression<Func<TSource, TKey>> keySelector, 
            bool isDescending = false)
        {
            //var provider = (collection as QueryableSource<TSource>).Provider;
            var typeArgs = new Type[] { typeof(TSource), typeof(TKey) };
            MethodInfo method = null;

            if (isDescending)
            {

                method = typeof(Enumerable).GetMethods().Where(m => m.Name.Contains("OrderByDescending"))
                    .FirstOrDefault().MakeGenericMethod(typeArgs);                
            }
            else
            {
                method = typeof(Enumerable).GetMethods().Where(m => m.Name.Contains("OrderBy"))
                       .FirstOrDefault().MakeGenericMethod(typeArgs);
            }

            var expr = Expression.Call(method, Expression.Constant(collection), keySelector);
            Provider(collection)?.CreateQuery(expr);

            return collection;
        }

        public static IEnumerable<TSource> AddLimit<TSource>(this IEnumerable<TSource> collection, ConstantExpression count, ConstantExpression offset)
        {
            //var provider = (collection as QueryableSource<TSource>).Provider;
            var type = typeof(TSource);
            var countVal = (int)count.Value;
            var offsetVal = (int)offset.Value;

            if (countVal > 0)
            {
                var takeMethod = typeof(Enumerable).GetMethods().Where(m => m.Name.Contains("Take"))
                    .FirstOrDefault().MakeGenericMethod(type);

                var takeExpr = Expression.Call(takeMethod, Expression.Constant(collection), count);
                Provider(collection)?.CreateQuery(takeExpr);
            };

            if (offsetVal > 0)
            {
                var skipMethod = typeof(Enumerable).GetMethods().Where(m => m.Name.Contains("Skip"))
                    .FirstOrDefault().MakeGenericMethod(type);

                var skipExpr = Expression.Call(skipMethod, Expression.Constant(collection), offset);
                Provider(collection)?.CreateQuery(skipExpr);
            }

            return collection;
        }

        public static IEnumerable<TSource> AddLimit<TSource>(this IEnumerable<TSource> collection, int count, int offset = 0)
        {
            return collection.AddLimit(Expression.Constant(count), Expression.Constant(offset));
        }

        public static IEnumerable<TSource> AddOffset<TSource>(this IEnumerable<TSource> collection, int offset)
        {
            return collection.AddLimit(Expression.Constant(0), Expression.Constant(offset));
        }

        public static int GeCountOf<TSource>(this IEnumerable<TSource> collection)
        {
            //var method = typeof(Enumerable).GetMethods().Where(m => m.Name.Contains("Where")).FirstOrDefault().MakeGenericMethod(typeof(T));
            return collection.Count();
        }


        public static IEnumerable<T> Example<T>(this IOrderedQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            return source.Where(predicate).ToList();
        }
    }
}
