using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Filters;

public class TimeOnlyConditionBuilder : ConditionBuilder
{
    public override TypeFilter SupportedType => TypeFilter.Time;
    #region Public Methods

    /// <inheritdoc/>

    public override Expression BuildCondition(Expression property, FilterRequestDto filter)
    {
        switch (filter.FilterOperator)
        {
            case "GreaterThan":
                return BuildTimeOnlyGreaterThanCondition(property, filter);
            case "GreaterThanOrEqual":
                return BuildTimeOnlyGreaterThanOrEqualCondition(property, filter);
            case "LessThan":
                return BuildTimeOnlyLessThanCondition(property, filter);
            case "LessThanOrEqual":
                return BuildTimeOnlyLessThanOrEqualCondition(property, filter);
            default:
                return BuildTimeOnlyBetweenCondition(property, filter);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Builds an expression that checks if the given property value is within a specified time range (inclusive).
    /// </summary>
    /// <param name="property">The property expression to check.</param>
    /// <param name="filter">The filter containing the start and end times for the range.</param>
    /// <returns>An expression that checks if the property value is between the specified start and end times.</returns>
    private static Expression BuildTimeOnlyBetweenCondition(Expression property, FilterRequestDto filter)
    {
        Expression startDateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));
        Expression endDateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));

        ConstantExpression startDateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.StartTime.Value), typeof(TimeOnly));
        ConstantExpression endDateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.EndTime.Value), typeof(TimeOnly));
        var greaterThan = Expression.GreaterThanOrEqual(startDateNonNullableProperty, startDateValueExpression);
        var lessThan = Expression.LessThanOrEqual(endDateNonNullableProperty, endDateValueExpression);
        return Expression.AndAlso(greaterThan, lessThan);

    }
    /// <summary>
    /// Builds an expression that checks if the given property value is greater than a specified time value.
    /// </summary>
    /// <param name="property">The property expression to check.</param>
    /// <param name="filter">The filter containing the time value to compare against.</param>
    /// <returns>An expression that checks if the property value is greater than the specified time.</returns>
    private static Expression BuildTimeOnlyGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));

        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression that checks if the given property value is greater than or equal to a specified time value.
    /// </summary>
    /// <param name="property">The property expression to check.</param>
    /// <param name="filter">The filter containing the time value to compare against.</param>
    /// <returns>An expression that checks if the property value is greater than or equal to the specified time.</returns>
    private static Expression BuildTimeOnlyGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));

        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression that checks if the given property value is less than a specified time value.
    /// </summary>
    /// <param name="property">The property expression to check.</param>
    /// <param name="filter">The filter containing the time value to compare against.</param>
    /// <returns>An expression that checks if the property value is less than the specified time.</returns>
    private static Expression BuildTimeOnlyLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));
        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }
    /// <summary>
    /// Builds an expression that checks if the given property value is less than or equal to a specified time value.
    /// </summary>
    /// <param name="property">The property expression to check.</param>
    /// <param name="filter">The filter containing the time value to compare against.</param>
    /// <returns>An expression that checks if the property value is less than or equal to the specified time.</returns>
    private static Expression BuildTimeOnlyLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));
        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    #endregion
}
