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
    private static Expression BuildDateOnlyLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));
        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }

    private static Expression BuildDateOnlyLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));
        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildDateOnlyGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));

        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
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
    private static Expression BuildDateOnlyGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));

        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }
}
