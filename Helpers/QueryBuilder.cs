using Cassandra.NET.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cassandra.NET.Helpers
{
    static class QueryBuilder
    {
        public static Query EvaluateQuery<T>(Expression<Func<T, bool>> predicate)
        {
            var expression = predicate.Body as BinaryExpression;
            if (expression != null)
            {
                var q = EvaluateBinaryExpression(expression);
                return q;
            }

            return null;
        }

        private static object VisitMemberAccess(MemberExpression expression)
        {
            // there should only be two options. PropertyInfo or FieldInfo... let's extract the VALUE accordingly
            var value = new object();
            if ((expression.Member as PropertyInfo) != null)
            {
                var exp = (MemberExpression)expression.Expression;
                var constant = (ConstantExpression)exp.Expression;
                var fieldInfoValue = ((FieldInfo)exp.Member).GetValue(constant.Value);
                value = ((PropertyInfo)expression.Member).GetValue(fieldInfoValue, null);
            }
            else if ((expression.Member as FieldInfo) != null)
            {
                var fieldInfo = expression.Member as FieldInfo;
                var constantExpression = expression.Expression as ConstantExpression;
                if (fieldInfo != null & constantExpression != null)
                {
                    value = fieldInfo.GetValue(constantExpression.Value);
                }
            }

            return value;
        }

        public static object GetValue(MemberExpression memberExpression)
        {
            var constant = (ConstantExpression)memberExpression.Expression;
            var fieldInfoValue = ((FieldInfo)memberExpression.Member).GetValue(constant.Value);
            var value = ((PropertyInfo)memberExpression.Member).GetValue(fieldInfoValue, null);

            return value;
        }



        private static Query EvaluateBinaryExpression(BinaryExpression binaryExpression)
        {
            object value = null;
            string queryString = null, memberName = null;
            var operand = GetOperand(binaryExpression.NodeType);

            if (binaryExpression.Left is BinaryExpression && binaryExpression.Right is BinaryExpression)
            {
                var left = EvaluateBinaryExpression((BinaryExpression)binaryExpression.Left);
                var right = EvaluateBinaryExpression((BinaryExpression)binaryExpression.Right);
                queryString = $"{left.Statment} {operand} {right.Statment}";
                return new Query(queryString, left.Values.Concat(right.Values).ToArray());
            }
            else
            {
                var expressions = new[] { binaryExpression.Left, binaryExpression.Right };
                foreach (var exp in expressions)
                {
                    if (exp is MemberExpression)
                    {
                        var memberExpression = (MemberExpression)exp;
                        if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            var property = memberExpression.Member as PropertyInfo;
                            if (property != null)
                                memberName = property.GetColumnNameMapping();
                            else
                                memberName = memberExpression.Member.Name;
                        }
                        else
                        {
                            value = VisitMemberAccess(memberExpression);
                        }

                        continue;
                    }

                    if (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.Constant)
                    {
                        value = GetValue(binaryExpression.Right);
                        continue;
                    }
                }
            }

            queryString = $"{memberName} {operand} ?";

            return new Query(queryString, new[] { value });
        }

        private static string GetOperand(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Not:
                    return "!";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Or:
                    return "or";
                default:
                    throw new NotSupportedException($"Node type {nodeType} is a valid operand");
            }
        }

        private static object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}