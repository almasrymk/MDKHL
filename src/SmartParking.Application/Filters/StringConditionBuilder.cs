using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Filters;

public class StringConditionBuilder : ConditionBuilder
{
    public override TypeFilter SupportedType => TypeFilter.Text;
    public override Expression BuildCondition(Expression property, FilterRequestDto filter)
    {
        ConstantExpression value = Expression.Constant(filter.FilterValue);
        if (string.IsNullOrWhiteSpace(filter.FilterOperator))
        {
            return Expression.Equal(property, value);
        }
        MethodInfo? toLowerMethodInfo;
        MethodCallExpression propertyToLower;
        ToLowerMethod(property, out toLowerMethodInfo, out propertyToLower);
        if (filter.FilterValues.Count > 0)
        {
            return BuildInMethod(filter, propertyToLower);
        }

        var valueToLower = Expression.Call(value, toLowerMethodInfo);
        var containsMethodInfo = typeof(string).GetMethod(filter.FilterOperator, new[] { typeof(string) });
        var condition = filter.IsNot
            ? GetNegatedCondition(property, propertyToLower, valueToLower, containsMethodInfo, filter)
            : GetPositiveCondition(property, propertyToLower, valueToLower, containsMethodInfo, filter);
        return condition;
    }

    private static void ToLowerMethod(Expression property, out MethodInfo? toLowerMethodInfo, out MethodCallExpression propertyToLower)
    {
        toLowerMethodInfo = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
        propertyToLower = Expression.Call(property, toLowerMethodInfo);
    }
    private static Expression BuildInMethod(FilterRequestDto filter, MethodCallExpression propertyToLower)
    {
        var lowerValues = filter.FilterValues.Select(s => s.ToLower()).ToList();
        var valuesConstant = Expression.Constant(lowerValues);
        var containsMethod = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
        var anyMethod = Expression.Call(valuesConstant, containsMethod, propertyToLower);
        return anyMethod;
    }

    private static Expression GetNegatedCondition(Expression property, Expression propertyToLower, Expression valueToLower, MethodInfo containsMethodInfo, FilterRequestDto filter)
    {
        return filter.FilterOperator == "IsNullOrEmpty"
            ? Expression.Not(Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property))
            : Expression.Not(Expression.Call(propertyToLower, containsMethodInfo, valueToLower));
    }

    private static Expression GetPositiveCondition(Expression property, Expression propertyToLower, Expression valueToLower, MethodInfo containsMethodInfo, FilterRequestDto filter)
    {
        return filter.FilterOperator == "IsNullOrEmpty"
            ? Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property)
            : Expression.Call(propertyToLower, containsMethodInfo, valueToLower);
    }
}
