

using System;
using System.Linq.Expressions;

namespace SmartParking.Filters;

public class NumberConditionBuilder : ConditionBuilder
{
    public override TypeFilter SupportedType => TypeFilter.Number;
    #region Public Methods
    /// <inheritdoc/>
   
    public override Expression BuildCondition(Expression property, FilterRequestDto filter)
    {
        switch (filter.FilterOperator)
        {
            case "GreaterThan":
                return BuildNumberGreaterThanCondition(property, filter);
            case "LessThan":
                return BuildNumberLessThanCondition(property, filter);
            default:
                return BuildNumberEqualCondition(property, filter);
        }
    }
    #endregion
    #region Private Methods
    /// <summary>
    /// Builds an expression for an "Equal" condition comparing a property with a given number value (int or decimal).
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the number value to be compared against the property.</param>
    /// <returns>An expression representing the "Equal" condition for the property and filter value.</returns>
    /// <exception cref="ArgumentException">Thrown when the filter value is not a valid number or the property type is unsupported.</exception>
    private static Expression BuildNumberEqualCondition(Expression property, FilterRequestDto filter)
    {
        return property.Type switch
        {
            Type t when t == typeof(int) && int.TryParse(filter.FilterValue, out int intValue)
                => Expression.Equal(property, Expression.Constant(intValue, typeof(int))),

            Type t when t == typeof(int?) && int.TryParse(filter.FilterValue, out int intValue)
                => Expression.Equal(property, Expression.Constant(intValue, typeof(int?))),

            Type t when t == typeof(decimal) && decimal.TryParse(filter.FilterValue, out decimal decimalValue)
                => Expression.Equal(property, Expression.Constant(decimalValue, typeof(decimal))),

            Type t when t == typeof(decimal?) && decimal.TryParse(filter.FilterValue, out decimal decimalValue)
                => Expression.Equal(property, Expression.Constant(decimalValue, typeof(decimal?))),

            _ => throw new ArgumentException("Invalid number format or unsupported type.")
        };
    }
    /// <summary>
    /// Builds an expression for a "LessThan" condition comparing a property with a given number value (int or decimal).
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the number value to be compared against the property.</param>
    /// <returns>An expression representing the "LessThan" condition for the property and filter value.</returns>
    /// <exception cref="ArgumentException">Thrown when the filter value is not a valid number or the property type is unsupported.</exception>
    private static Expression BuildNumberLessThanCondition(Expression property, FilterRequestDto filter)
    {
        return property.Type switch
        {
            Type t when t == typeof(int) && int.TryParse(filter.FilterValue, out int intValue)
                => Expression.LessThan(property, Expression.Constant(intValue, typeof(int))),

            Type t when t == typeof(int?) && int.TryParse(filter.FilterValue, out int intValue)
                => Expression.LessThan(property, Expression.Constant(intValue, typeof(int?))),

            Type t when t == typeof(decimal) && decimal.TryParse(filter.FilterValue, out decimal decimalValue)
                => Expression.LessThan(property, Expression.Constant(decimalValue, typeof(decimal))),

            Type t when t == typeof(decimal?) && decimal.TryParse(filter.FilterValue, out decimal decimalValue)
                => Expression.LessThan(property, Expression.Constant(decimalValue, typeof(decimal?))),

            _ => throw new ArgumentException("Invalid number format or unsupported type.")
        };
    }
    /// <summary>
    /// Builds an expression for a "GreaterThan" condition comparing a property with a given number value (int or decimal).
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the number value to be compared against the property.</param>
    /// <returns>An expression representing the "GreaterThan" condition for the property and filter value.</returns>
    /// <exception cref="ArgumentException">Thrown when the filter value is not a valid number or the property type is unsupported.</exception>
    private static Expression BuildNumberGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        return property.Type switch
        {
            Type t when t == typeof(int) && int.TryParse(filter.FilterValue, out int intValue)
                => Expression.GreaterThan(property, Expression.Constant(intValue, typeof(int))),

            Type t when t == typeof(int?) && int.TryParse(filter.FilterValue, out int intValue)
                => Expression.GreaterThan(property, Expression.Constant(intValue, typeof(int?))),

            Type t when t == typeof(decimal) && decimal.TryParse(filter.FilterValue, out decimal decimalValue)
                => Expression.GreaterThan(property, Expression.Constant(decimalValue, typeof(decimal))),

            Type t when t == typeof(decimal?) && decimal.TryParse(filter.FilterValue, out decimal decimalValue)
                => Expression.GreaterThan(property, Expression.Constant(decimalValue, typeof(decimal?))),

            _ => throw new ArgumentException("Invalid number format or unsupported type.")
        };
    }
    #endregion
}
