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
        private Expression _LambdaExpression;
        private dynamic _Parameter;

        public IEnumerable<Condition> Conditions => _Conditions.AsReadOnly();

        public override Expression Visit(Expression node)
        {
            if (_LambdaExpression == null) //Quote -> Lambda -> Call
            {
                _LambdaExpression = node;
                //_Parameter = ((dynamic)_LambdaExpression).Operand.Parameters[0];
                _Parameter = ((dynamic)_LambdaExpression).Parameters[0];
            }

            if (node is LambdaExpression)
            {
                var body = ((LambdaExpression)node).Body;
            }

            //if (node.NodeType == ExpressionType.Call)
            //    VisitMethodCall(node as MethodCallExpression);

            //VisitMethodCall(node as MethodCallExpression);

            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var lambda = Expression.Lambda(node, _Parameter);

            if (lambda.Body is BinaryExpression)
            {
                _Conditions.Add(CreateCondition((BinaryExpression)lambda.Body));
            }

            if (lambda.Body is MethodCallExpression)
                _Conditions.Add(CreateCondition((MethodCallExpression)lambda.Body));            

            return base.VisitMethodCall(node);
        }

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
                var condition = new Condition { Operator = operation };
                var whereVisitor = new WhereExpressionVisitor();
                whereVisitor._LambdaExpression = _LambdaExpression;
                whereVisitor.Visit(node.Left);
                whereVisitor.Visit(node.Right);
                _Conditions.AddRange(whereVisitor.Conditions);

                return node;
            }
            else
            {
                var condition = new Condition();
                var member = node.Left as MemberExpression;

                if (member == null)
                {
                    var unaryMember = node.Left as UnaryExpression;
                    if (unaryMember != null)
                        member = unaryMember.Operand as MemberExpression;
                }

                if (member != null)                
                    condition.LeftOperand = member.Member.Name;

                condition.Operator = Expression.Lambda(node, _Parameter);

                var valueNode = _Parameter != null && _Parameter == (node.Right as MemberExpression)?.Expression
                    ? node.Left
                    : node.Right;
                condition.RightOperand = GetValueFromExpression(valueNode);
                _Conditions.Add(condition);
                return node;
            }
        }

        private static object GetValueFromExpression(Expression node)
        {
            var member = node as MemberExpression;

            if (member == null)
            {
                var unaryMember = node as UnaryExpression;
                if (unaryMember != null)
                {
                    member = unaryMember.Operand as MemberExpression;
                }
            }

            if (member != null)
                return Expression.Lambda(member).Compile().DynamicInvoke();

            var constant = node as ConstantExpression;
            if (constant != null)
                return constant.Value;

            throw new NotImplementedException();
        }

        private Condition CreateCondition(MethodCallExpression exp)
        {
            var methodName = exp.Method.Name;
            var allMethods = exp.Method.DeclaringType.GetMethods();
            var method = allMethods.Where(m => m.Name.Equals(methodName)).FirstOrDefault();

            if (methodName.Equals("Equals") && method.DeclaringType.Name.Equals("String"))
                return CreateCondition(BinaryExpression.Equal(exp.Object, exp.Arguments.FirstOrDefault()));

            throw new NotImplementedException($"this call method is not suported [{exp.NodeType}]");
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
                        Condition left = CreateCondition((BinaryExpression)exp.Left);
                        Condition right = CreateCondition((BinaryExpression)exp.Right);
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
