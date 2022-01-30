using OrmLight.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OrmLight.Linq.Visitors
{
    class WhereExpressionVisitor : ExpressionVisitor
    {
        private List<Condition> _Conditions = new List<Condition>();     
        public IEnumerable<Condition> Conditions => _Conditions.AsReadOnly();

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var operation = Condition.GetOperator(node.NodeType);

            if (operation == ConditionOperator.Equal)
            {
                _Conditions.Add(CreateCondition(node));
                return node;
            }
            else if (operation == ConditionOperator.And)
            {
                Visit(node.Left);
                Visit(node.Right);

                return node;
            }
            else if (operation == ConditionOperator.Or)
            {
                _Conditions.Add(CreateCondition(node));
                return node;
            }
            

           return node;            
        }       

        private Condition CreateCondition(MethodCallExpression expr)
        {
            var methodName = expr.Method.Name;
            var allMethods = expr.Method.DeclaringType.GetMethods();
            var method = allMethods.Where(m => m.Name.Equals(methodName)).FirstOrDefault();

            if (methodName.Equals("Equals") && method.DeclaringType.Name.Equals("String"))
                return CreateCondition(Expression.Equal(expr.Object, expr.Arguments.FirstOrDefault()));

            throw new NotImplementedException($"this call method is not suported [{methodName}]");
        }

        private Condition CreateCondition(BinaryExpression exp)
        {
            Condition condition = null;
            var op = Condition.GetOperator(exp.NodeType);

            switch (exp.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                    {
                        var left = (MemberExpression)exp.Left;
                        var right = (ConstantExpression)exp.Right;
                        condition = new Condition()
                        {
                            LeftOperand = left.Member.Name,
                            Operator = op,
                            RightOperand = right.Value
                        };
                        break;
                    }
                case ExpressionType.OrElse:
                case ExpressionType.AndAlso:
                    {
                        MethodCallExpression leftCall = exp.Left as MethodCallExpression;
                        Condition left = leftCall != null ? CreateCondition(leftCall) : CreateCondition((BinaryExpression)exp.Left);

                        MethodCallExpression rightCall = exp.Right as MethodCallExpression;
                        Condition right = rightCall != null ? CreateCondition(rightCall) : CreateCondition((BinaryExpression)exp.Right);

                        condition = new Condition()
                        {
                            LeftOperand = left,
                            Operator = op,
                            RightOperand = right
                        };
                        break;
                    }
                default:
                    throw new NotImplementedException($"unknown expression type [{exp.NodeType}]");
            }

            return condition;
        }
    }
}
