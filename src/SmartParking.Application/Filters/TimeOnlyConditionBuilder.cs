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
    private static Expression BuildTimeOnlyGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));

        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildTimeOnlyGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));

        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildTimeOnlyLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));
        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildTimeOnlyLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));
        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
}
