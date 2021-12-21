using OrmLight.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OrmLight.Linq
{
    public class QueryableSource<TEntity> : IOrderedQueryable<TEntity>
    {
        private readonly Expression _Expression;
        private readonly QueryProvider<TEntity> _Provider;

        public Expression Expression => _Expression;
        public Type ElementType => typeof(TEntity);
        public IQueryProvider Provider => _Provider;


        public QueryableSource(IDataAccessLayer dal, Operation operation)
        {
                
        }
        public IEnumerator<TEntity> GetEnumerator() => _Provider.GetEnumerable<TEntity>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
