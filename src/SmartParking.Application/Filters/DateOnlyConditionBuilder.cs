using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Filters;

public class DateOnlyConditionBuilder : ConditionBuilder
{
    public override TypeFilter SupportedType => TypeFilter.DateOnly;
    #region Public Methods
    /// <inheritdoc/>
    public override Expression BuildCondition(Expression property, FilterRequestDto filter)
    {
        switch (filter.FilterOperator)
        {
            case "GreaterThan":
                return BuildDateOnlyGreaterThanCondition(property, filter);
            case "GreaterThanOrEqual":
                return BuildDateOnlyGreaterThanOrEqualCondition(property, filter);
            case "LessThan":
                return BuildDateOnlyLessThanCondition(property, filter);
            case "LessThanOrEqual":
                return BuildDateOnlyLessThanOrEqualCondition(property, filter);
            default:
                return BuildDateOnlyBetweenCondition(property, filter);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Builds an expression for a "LessThan" condition comparing a property with a DateOnly value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the DateOnly value for comparison.</param>
    /// <returns>An expression representing the "LessThan" condition for DateOnly values.</returns>
    private static Expression BuildDateOnlyLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));
        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression for a "LessThanOrEqual" condition comparing a property with a DateOnly value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the DateOnly value for comparison.</param>
    /// <returns>An expression representing the "LessThanOrEqual" condition for DateOnly values.</returns>
    private static Expression BuildDateOnlyLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));
        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression for a "GreaterThanOrEqual" condition comparing a property with a DateOnly value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the DateOnly value for comparison.</param>
    /// <returns>An expression representing the "GreaterThanOrEqual" condition for DateOnly values.</returns>
    private static Expression BuildDateOnlyGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));

        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression for a "Between" condition comparing a property with a range of DateOnly values.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the start and end DateOnly values for comparison.</param>
    /// <returns>An expression representing the "Between" condition for DateOnly values.</returns>
    private static Expression BuildDateOnlyBetweenCondition(Expression property, FilterRequestDto filter)
    {
        Expression startDateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));
        Expression endDateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));

        ConstantExpression startDateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.StartDate), typeof(DateOnly));
        ConstantExpression endDateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.EndDate), typeof(DateOnly));
        var greaterThan = Expression.GreaterThanOrEqual(startDateNonNullableProperty, startDateValueExpression);
        var lessThan = Expression.LessThanOrEqual(endDateNonNullableProperty, endDateValueExpression);
        return Expression.AndAlso(greaterThan, lessThan);

    }
    /// <summary>
    /// Builds an expression for a "GreaterThan" condition comparing a property with a DateOnly value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the DateOnly value for comparison.</param>
    /// <returns>An expression representing the "GreaterThan" condition for DateOnly values.</returns>
    private static Expression BuildDateOnlyGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));

        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }
    #endregion
}
