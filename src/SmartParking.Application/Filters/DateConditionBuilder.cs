using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Filters;

public class DateConditionBuilder : ConditionBuilder
{
    public override TypeFilter SupportedType => TypeFilter.Date;
    #region Public Methods
     /// <inheritdoc/>
    public override Expression BuildCondition(Expression property, FilterRequestDto filter)
    {
        switch (filter.FilterOperator)
        {
            case "GreaterThan":
                return BuildDateGreaterThanCondition(property, filter);
            case "GreaterThanOrEqual":
                return BuildDateGreaterThanOrEqualCondition(property, filter);
            case "LessThan":
                return BuildDateLessThanCondition(property, filter);
            case "LessThanOrEqual":
                return BuildDateLessThanOrEqualCondition(property, filter);
            default:
                return BuildDateBetweenCondition(property, filter);
        }
    }
    #endregion
    #region Private Methods
    /// <summary>
    /// Builds an expression for a "LessThanOrEqual" condition comparing a property with a date value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the date value for comparison.</param>
    /// <returns>An expression representing the "LessThanOrEqual" condition.</returns>
    private static Expression BuildDateLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));
        ConstantExpression DateValueExpression = Expression.Constant(filter.Date.AddDays(1), typeof(DateTime));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression for a "LessThan" condition comparing a property with a date value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the date value for comparison.</param>
    /// <returns>An expression representing the "LessThan" condition.</returns>
    private static Expression BuildDateLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));
        ConstantExpression DateValueExpression = Expression.Constant(filter.Date, typeof(DateTime));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression for a "Between" condition comparing a property with a date range.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the start and end date values for comparison.</param>
    /// <returns>An expression representing the "Between" condition.</returns>
    private static Expression BuildDateBetweenCondition(Expression property, FilterRequestDto filter)
    {
        Expression startDateNonNullableProperty = Expression.Convert(property, typeof(DateTime));
        Expression endDateNonNullableProperty = Expression.Convert(property, typeof(DateTime));

        ConstantExpression startDateValueExpression = Expression.Constant(filter.StartDate, typeof(DateTime));
        ConstantExpression endDateValueExpression = Expression.Constant(filter.EndDate.AddDays(1), typeof(DateTime));
        var greaterThan = Expression.GreaterThanOrEqual(startDateNonNullableProperty, startDateValueExpression);
        var lessThan = Expression.LessThanOrEqual(endDateNonNullableProperty, endDateValueExpression);
        return Expression.AndAlso(greaterThan, lessThan);

    }
    /// <summary>
    /// Builds an expression for a "GreaterThan" condition comparing a property with a date value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the date value for comparison.</param>
    /// <returns>An expression representing the "GreaterThan" condition.</returns>
    private static Expression BuildDateGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));

        ConstantExpression DateValueExpression = Expression.Constant(filter.Date.AddDays(1), typeof(DateTime));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression for a "GreaterThanOrEqual" condition comparing a property with a date value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the date value for comparison.</param>
    /// <returns>An expression representing the "GreaterThanOrEqual" condition.</returns>
    private static Expression BuildDateGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));

        ConstantExpression DateValueExpression = Expression.Constant(filter.Date, typeof(DateTime));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    #endregion
}
