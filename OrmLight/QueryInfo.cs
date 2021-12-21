using OrmLight.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight
{
    public class QueryInfo
    {
        public Operation Operation { get; set; }
        public Type EntityType { get; set; }
        public List<Condition> Conditions { get; set; }
        public List<Sorting> Sortings { get; set; }
        public List<Limit> Limits { get; set; }

        public QueryInfo()
        {
            Conditions = new List<Condition>();
            Sortings = new List<Sorting>();
            Limits = new List<Limit>();
        }
    }

    public class Condition
    {
        public object LeftOperand { get; set; }
        public ConditionOperator Operator { get; set; }
        public object RightOperand { get; set; }
        public static Condition Equal => new Condition() { Operator = ConditionOperator.Equal };


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

    public class Sorting
    {
        public string FieldName { get; set; }
        public bool IsDesc { get; set; }
    }

    public class Limit
    {
        public int Count { get; set; }
        public int Offset { get; set; }
    }
}
