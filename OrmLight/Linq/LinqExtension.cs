using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight.Linq
{
    public static class LinqExtension
    {
        private static Expression<Func<T, bool>> FuncToExpression<T>(Func<T, bool> call)
        {
            MethodCallExpression methodCall = call.Target == null
                ? Expression.Call(call.Method)
                : Expression.Call(Expression.Constant(call.Target), call.Method);

            //MethodCallExpression methodCall = Expression.Call(target, call.Method);

            //Expression.Call()

            return Expression.Lambda<Func<T,bool>>(methodCall);
        }

        public static IEnumerable<T> AddCondition<T>(this IEnumerable<T> collection, Expression<Func<T, bool>> predicate)
        {
            var method = typeof(Enumerable).GetMethods().Where(m => m.Name.Contains("Where")).FirstOrDefault().MakeGenericMethod(typeof(T)); //TODO
            //Expression<Func<T, bool>> lambda = input => predicate(input);

            //lambda.Compile();
            ////var quote = Expression.Quote(lambda);

            //var expr = Expression.Call(method, Expression.Constant(collection), lambda);
            var expr = Expression.Call(method, Expression.Constant(collection), predicate);
            var provider = (collection as QueryableSource<T>).Provider;
            provider.CreateQuery(expr);

            return collection;
        }

        public static IEnumerable<T> Execute<T>(this IEnumerable<T> collection)
        {
            //var source = (QueryableSource<T>)collection;
            //var claims = (source.Provider as QueryProvider<T>).Claims;
            //claims = claims.ToList();

            return collection.ToList();
        }

        public static IEnumerable<TSource> AddSort<TSource, TKey>(this IEnumerable<TSource> collection, Func<TSource, TKey> keySelector, bool isDescending = false)
        {
            if (isDescending)
                return collection.OrderByDescending(keySelector);

            return collection.OrderBy(keySelector);
        }

        public static IEnumerable<TSource> AddLimit<TSource>(this IEnumerable<TSource> collection, int count, int offset = 0)
        {
            var newCollection = collection.Take(count);
            if (offset > 0)
                newCollection.Skip(offset);

            return newCollection;
        }

        public static int GetNumberOf<TSource>(this IEnumerable<TSource> collection)
        {
            return collection.Count();
        }
    }
}
