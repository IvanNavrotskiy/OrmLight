using OrmLight.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight.Linq
{
    class QueryProvider<TEntity> : IQueryProvider
    {
        private IDataAccessLayer _DAL;
        private Operation _Operation;

        public QueryProvider(IDataAccessLayer dal, Operation operation)
        {
            _DAL = dal;
            _Operation = operation;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            //var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            //var results = _dataQuery(queryVisitor.QueryInfo);
            //return (IEnumerable<TResult>)results;

            return Enumerable.Empty<TResult>();
        }
    }
}
