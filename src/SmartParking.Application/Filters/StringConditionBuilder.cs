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
    #region Public Methods

    /// <inheritdoc/>
   
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
    #endregion
    #region Private Methods
    /// <summary>
    /// Retrieves the method info for the <see cref="string.ToLower"/> method and generates a method call expression.
    /// </summary>
    /// <param name="property">The property expression to call <see cref="string.ToLower"/> on.</param>
    /// <param name="toLowerMethodInfo">The method info of <see cref="string.ToLower"/>.</param>
    /// <param name="propertyToLower">The method call expression for <see cref="string.ToLower"/> on the property.</param>
    private static void ToLowerMethod(Expression property, out MethodInfo? toLowerMethodInfo, out MethodCallExpression propertyToLower)
    {
        toLowerMethodInfo = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
        propertyToLower = Expression.Call(property, toLowerMethodInfo);
    }
    /// <summary>
    /// Builds an expression to check if the property value is included in a list of lowercase filter values.
    /// </summary>
    /// <param name="filter">The filter request containing the list of filter values.</param>
    /// <param name="propertyToLower">The method call expression for the property after applying <see cref="string.ToLower"/>.</param>
    /// <returns>An expression that checks if the lowercase property value is contained in the list of lowercase filter values.</returns>
    private static Expression BuildInMethod(FilterRequestDto filter, MethodCallExpression propertyToLower)
    {
        var lowerValues = filter.FilterValues.Select(s => s.ToLower()).ToList();
        var valuesConstant = Expression.Constant(lowerValues);
        var containsMethod = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
        var anyMethod = Expression.Call(valuesConstant, containsMethod, propertyToLower);
        return anyMethod;
    }
    /// <summary>
    /// Builds a negated condition based on the provided filter and property, either checking for non-empty strings or negating a containment check.
    /// </summary>
    /// <param name="property">The property expression to be evaluated.</param>
    /// <param name="propertyToLower">The property expression with <see cref="string.ToLower"/> applied to it.</param>
    /// <param name="valueToLower">The lowercase value from the filter to compare against the property.</param>
    /// <param name="containsMethodInfo">The method info for the <see cref="List{T}.Contains"/> method.</param>
    /// <param name="filter">The filter request containing the condition operator and value.</param>
    /// <returns>An expression representing the negated condition based on the filter operator.</returns>
    private static Expression GetNegatedCondition(Expression property, Expression propertyToLower, Expression valueToLower, MethodInfo containsMethodInfo, FilterRequestDto filter)
    {
        return filter.FilterOperator == "IsNullOrEmpty"
            ? Expression.Not(Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property))
            : Expression.Not(Expression.Call(propertyToLower, containsMethodInfo, valueToLower));
    }
    /// <summary>
    /// Builds a positive condition based on the provided filter and property, either checking for empty strings or checking if the property contains the value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated.</param>
    /// <param name="propertyToLower">The property expression with <see cref="string.ToLower"/> applied to it.</param>
    /// <param name="valueToLower">The lowercase value from the filter to compare against the property.</param>
    /// <param name="containsMethodInfo">The method info for the <see cref="List{T}.Contains"/> method.</param>
    /// <param name="filter">The filter request containing the condition operator and value.</param>
    /// <returns>An expression representing the positive condition based on the filter operator.</returns>
    private static Expression GetPositiveCondition(Expression property, Expression propertyToLower, Expression valueToLower, MethodInfo containsMethodInfo, FilterRequestDto filter)
    {
        return filter.FilterOperator == "IsNullOrEmpty"
            ? Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property)
            : Expression.Call(propertyToLower, containsMethodInfo, valueToLower);
    }
    #endregion
}
