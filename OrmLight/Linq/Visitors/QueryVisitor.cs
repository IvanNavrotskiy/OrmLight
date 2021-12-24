using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight.Linq.Visitors
{
    public class QueryVisitor : ExpressionVisitor
    {
        public QueryInfo QueryInfo { get; private set; }

        public QueryVisitor(QueryInfo queryInfo = null)
        {
            QueryInfo = queryInfo ?? new QueryInfo();
        }

        [return: NotNullIfNotNull("node")]
        public override Expression Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.Call)
            {
                return VisitMethodCall((MethodCallExpression)node);
            }
            else if (node.NodeType == ExpressionType.Constant && QueryInfo.EntityType == null)
            {
                var entityType = (node as ConstantExpression)?.Value.GetType().GenericTypeArguments?.FirstOrDefault();
                QueryInfo.EntityType = entityType;
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expr)
        {
            if ((expr.Method.DeclaringType == typeof(Queryable)) || (expr.Method.DeclaringType != typeof(Enumerable)))
            {
                if (expr.Method.Name.Equals("Skip"))
                {
                    Visit(expr.Arguments[0]);
                    var countExpression = (ConstantExpression)(expr.Arguments[1]);
                    QueryInfo.Limits.Add(new Limit() { Offset = (int)countExpression.Value });
                }
                if (expr.Method.Name.Equals("Take"))
                {
                    Visit(expr.Arguments[0]);
                    var countExpression = (ConstantExpression)(expr.Arguments[1]);
                    QueryInfo.Limits.Add(new Limit() { Count = (int)countExpression.Value });
                }
                if (expr.Method.Name.Equals("Where"))
                {
                    MethodCallExpression call = expr;
                    var whereExp = call.Arguments[1];
                    var whereVisitor = new WhereExpressionVisitor();
                    whereVisitor.Visit(whereExp);                    

                    QueryInfo.Conditions.AddRange(whereVisitor.Conditions);
                }
                if (expr.Method.Name.Equals("OrderBy"))
                {
                    MethodCallExpression call = expr;
                    var lambda = (LambdaExpression)_RemoveQuotes(call.Arguments[1]);
                    var lambdaBody = (MemberExpression)_RemoveQuotes(lambda.Body);
                    QueryInfo.Sortings.Add(new Sorting() { FieldName = lambdaBody.Member.Name, IsDesc = false });
                }
                if (expr.Method.Name.Equals("OrderByDescending"))
                {
                    MethodCallExpression call = expr;
                    var lambda = (LambdaExpression)_RemoveQuotes(call.Arguments[1]);
                    var lambdaBody = (MemberExpression)_RemoveQuotes(lambda.Body);
                    QueryInfo.Sortings.Add(new Sorting() { FieldName = lambdaBody.Member.Name, IsDesc = true });
                }
                if (expr.Method.Name.Equals("Count"))
                {
                    // temp
                    QueryInfo.CountOnly = true;
                }

                //foreach (var arg in expr.Arguments)
                //{
                //    Visit(arg);
                //}
            }            

            return expr;
        }

        private Expression _RemoveQuotes(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Quote)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            return expr;
        }
    }
}
