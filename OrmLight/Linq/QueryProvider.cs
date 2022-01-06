using OrmLight.Enums;
using OrmLight.Linq.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight.Linq
{
    public class QueryProvider<TEntity> : IQueryProvider
    {
        private IDataAccessLayer _DAL;
        private Operation _Operation;
        private readonly QueryVisitor _QueryVisitor;

        //public IEnumerable<TEntity> Claims { get; set; }

        public QueryProvider(IDataAccessLayer dal, Operation operation, QueryVisitor queryVisitor = null)
        {
            _DAL = dal;
            _Operation = operation;
            _QueryVisitor = queryVisitor ?? new QueryVisitor(operation, typeof(TEntity));
            //Claims = Enumerable.Empty<TEntity>();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            _QueryVisitor.Visit(expression);

            return new QueryableSource<TElement>(_DAL, _Operation, new QueryProvider<TElement>(_DAL, _Operation, _QueryVisitor), expression);
        }

        public QueryInfo GetQueryInfo()
        {
            return _QueryVisitor?.QueryInfo?.Clone() as QueryInfo;
        }

        public object Execute(Expression expression)
        {
            return Execute<TEntity>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            _QueryVisitor.Visit(expression);
            return _DAL.Execute<TResult>(GetQueryInfo());
        }

        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            //var queryVisitor = new QueryVisitor((QueryInfo)_QueryVisitor.QueryInfo.Clone());
            //var results = _dataQuery(queryVisitor.QueryInfo);
            return (IEnumerable<TResult>)_DAL.Execute<TResult>(_QueryVisitor.QueryInfo);
            //_QueryVisitor.Visit(expression);
            //return _DAL.Execute<TResult>(_QueryVisitor.QueryInfo.Clone() as QueryInfo);
        }
    }
}
