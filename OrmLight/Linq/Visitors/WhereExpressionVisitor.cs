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
            if (_LambdaExpression == null)
            {
                _LambdaExpression = node;
                _Parameter = ((dynamic)_LambdaExpression).Operand.Parameters[0];
            }
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var condition = new Condition();
            var memberExpression = ((MemberExpression)node.Object);
            condition.LeftOperand = memberExpression.Member.Name;
            condition.Operator = Expression.Lambda(node, _Parameter);
            condition.RightOperand = node.Arguments[0];
            _Conditions.Add(condition);

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var operation = Condition.GetOperator(node.NodeType);

            if (operation == ConditionOperator.And)
            {
                Visit((BinaryExpression)node.Left);
                Visit((BinaryExpression)node.Right);

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
    }
}
