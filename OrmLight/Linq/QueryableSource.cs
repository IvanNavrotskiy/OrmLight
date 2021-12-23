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

        public QueryableSource(IDataAccessLayer dal, Operation operation, QueryProvider<TEntity> provider = null, Expression expr = null)
        {            
            _Provider = provider ?? new QueryProvider<TEntity>(dal, operation);
            _Expression = expr ?? Expression.Constant(this);
        }

        //public IEnumerator<TEntity> GetEnumerator() => (_Provider.Execute<IEnumerable<TEntity>>(_Expression)).GetEnumerator();
        public IEnumerator<TEntity> GetEnumerator()  => this._Provider.GetEnumerable<TEntity>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
