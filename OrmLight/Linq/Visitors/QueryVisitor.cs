using OrmLight.Enums;
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

        public QueryVisitor(Operation operation, Type entityType, QueryInfo queryInfo = null)
        {
            QueryInfo = queryInfo ?? new QueryInfo(operation);
            QueryInfo.EntityType = entityType;
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
                // define the requested type
                var entityType = (node as ConstantExpression)?.Value.GetType().GenericTypeArguments?.FirstOrDefault();
                QueryInfo.EntityType = entityType;
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expr)
        {
            // ability to control work with interfaces here
            if ((expr.Method.DeclaringType == typeof(Queryable)) || (expr.Method.DeclaringType == typeof(Enumerable)))
            {
                switch (expr.Method.Name)
                {
                    case "Skip":
                        var skipCountExpression = (ConstantExpression)(expr.Arguments[1]);
                        QueryInfo.Limit = QueryInfo.Limit ?? new Limit();
                        QueryInfo.Limit.Offset += (int)skipCountExpression.Value;
                        break;
                    case "Take":
                        var takeCountExpression = (ConstantExpression)(expr.Arguments[1]);
                        QueryInfo.Limit = QueryInfo.Limit ?? new Limit();
                        QueryInfo.Limit.Count += (int)takeCountExpression.Value;
                        break;
                    case "Where":
                        if (expr.Arguments.Count > 1)
                        {
                            var whereExp = expr.Arguments[1];
                            var whereVisitor = new WhereExpressionVisitor();
                            whereVisitor.Visit(whereExp);
                            QueryInfo.Conditions.AddRange(whereVisitor.Conditions);
                        }
                        break;
                    case "OrderBy":
                        MethodCallExpression orderByCall = expr;
                        var orderByLambda = (LambdaExpression)_RemoveQuotes(orderByCall.Arguments[1]);
                        var orderByLambdaBody = (MemberExpression)_RemoveQuotes(orderByLambda.Body);
                        QueryInfo.Sortings.Add(new Sorting() 
                        { 
                            FieldName = orderByLambdaBody.Member.Name, IsDesc = false 
                        });
                        break;
                    case "OrderByDescending":
                        MethodCallExpression orderByDescCall = expr;
                        var orderByDescLambda = (LambdaExpression)_RemoveQuotes(orderByDescCall.Arguments[1]);
                        var orderByDescLambdaBody = (MemberExpression)_RemoveQuotes(orderByDescLambda.Body);
                        QueryInfo.Sortings.Add(new Sorting() 
                        { 
                            FieldName = orderByDescLambdaBody.Member.Name, IsDesc = true 
                        });
                        break;
                    case "Count":
                        // TODO: to think about perform predicate in default linq
                        QueryInfo.Operation = Operation.Count;
                        break;
                    default:
                        throw new NotImplementedException($"this method is not suported [{expr.Method.Name}]");
                }
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
