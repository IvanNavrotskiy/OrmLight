using OrmLight.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight
{
    public class QueryInfo : ICloneable
    {
        public Operation Operation { get; set; }
        public Type EntityType { get; set; }
        public List<Condition> Conditions { get; set; }
        public List<Sorting> Sortings { get; set; }
        public Limit Limit { get; set; }

        public QueryInfo(Operation operation)
        {
            Operation = operation;
            Conditions = new List<Condition>();
            Sortings = new List<Sorting>();
            Limit = new Limit();
        }

        public object Clone()
        {
            var other = (QueryInfo)this.MemberwiseClone();
            other.EntityType = EntityType;
            other.Operation = Operation;
            other.Conditions = Conditions?.Select(c => c?.Clone()).Cast<Condition>().ToList();
            other.Sortings = Sortings?.Select(s => s?.Clone()).Cast<Sorting>().ToList();
            other.Limit = Limit?.Clone() as Limit;

            return other;
        }
    }

    public class Condition : ICloneable
    {
        public object LeftOperand { get; set; }
        public ConditionOperator Operator { get; set; }
        public object RightOperand { get; set; }
        public static Condition Equal => new Condition() { Operator = ConditionOperator.Equal };

        public object Clone()
        {
            var other = (Condition)this.MemberwiseClone();

            var leftCondition = LeftOperand as Condition;
            if (leftCondition != null)
                other.LeftOperand = leftCondition.Clone();
            else
                other.LeftOperand = LeftOperand;

            var rightCondition = RightOperand as Condition;
            if (rightCondition != null)
                other.RightOperand = rightCondition.Clone();
            else
                other.RightOperand = RightOperand;

            other.Operator = Operator;

            return other;
        }

        public static ConditionOperator GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return ConditionOperator.Equal;
                case ExpressionType.GreaterThan:
                    return ConditionOperator.Greater;
                case ExpressionType.LessThan:
                    return ConditionOperator.Less;
                case ExpressionType.OrElse:
                    return ConditionOperator.Or;
                case ExpressionType.AndAlso:
                    return ConditionOperator.And;
                //TODO: etc 
                default:
                    throw new ApplicationException("unknown operator");
            }
        }
    }

    public class Sorting : ICloneable
    {
        public string FieldName { get; set; }
        public bool IsDesc { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class Limit : ICloneable
    {
        public int Count { get; set; }
        public int Offset { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
