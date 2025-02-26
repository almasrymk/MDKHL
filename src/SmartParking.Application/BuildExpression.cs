using FluentValidation.Validators;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartParking;

public static class BuildExpression<T>
{
    public static Expression<Func<T, bool>> BuildFilterExpression(List<FilterRequestDto> filterDatas)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        Expression combinedExpression = Expression.Constant(true);

        foreach (var filter in filterDatas)
        {
            if (string.IsNullOrEmpty(filter.FilterProperty) && string.IsNullOrEmpty(filter.FilterOperator))
                continue;

            Expression property = GetPropertyExpression(param, filter.FilterProperty);
            ConstantExpression value = Expression.Constant(filter.FilterValue);
            combinedExpression = Expression.AndAlso(combinedExpression, BuildCondition(property, filter, value));
        }

        return Expression.Lambda<Func<T, bool>>(combinedExpression, param);
    }


    private static Expression GetPropertyExpression(Expression param, string filterProperty)
    {
        Expression property = param;

        foreach (var part in filterProperty.Split('.'))
        {
            property = Expression.Property(property, part);
        }

        return property;
    }

    private static Expression BuildCondition(Expression property, FilterRequestDto filter, ConstantExpression value)
    {
        switch (filter.TypeFilter)
        {
            case TypeFilter.Enum:
                return BuildEnumCondition(property, filter);
            case TypeFilter.Number:
                return BuildNumberCondition(property, filter);
            case TypeFilter.Date:
                return BuildDateCondition(property, filter);            
            case TypeFilter.DateOnly:
                return BuildDateOnlyCondition(property, filter);            
            case TypeFilter.Time:
                return BuildTimeOnlyCondition(property, filter);
            default:
                return BuildStringCondition(property, filter, value);
        }
    }

    private static Expression BuildEnumCondition(Expression property, FilterRequestDto filter)
    {
        Type propertyType = property.Type;
        ConstantExpression valueNumber = Expression.Constant(Convert.ChangeType(Enum.Parse(propertyType, filter.FilterValue.ToString()), propertyType));
        return Expression.Equal(property, valueNumber);
    }

    private static Expression BuildNumberCondition(Expression property, FilterRequestDto filter)
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

    private static Expression BuildDateCondition(Expression property, FilterRequestDto filter)
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
    
    private static Expression BuildDateOnlyCondition(Expression property, FilterRequestDto filter)
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
    
    private static Expression BuildTimeOnlyCondition(Expression property, FilterRequestDto filter)
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

    private static Expression BuildDateGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));

        ConstantExpression DateValueExpression = Expression.Constant(filter.Date.AddDays(1), typeof(DateTime));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }

    private static Expression BuildDateOnlyGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));

        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }  
    private static Expression BuildTimeOnlyGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));

        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));

        return Expression.GreaterThan(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildDateGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));

        ConstantExpression DateValueExpression = Expression.Constant(filter.Date, typeof(DateTime));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }   
    
    private static Expression BuildDateOnlyGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));

        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildTimeOnlyGreaterThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));

        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));

        return Expression.GreaterThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildDateLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));
        ConstantExpression DateValueExpression = Expression.Constant(filter.Date, typeof(DateTime));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }    
    private static Expression BuildDateOnlyLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));
        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildTimeOnlyLessThanCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));
        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));
        return Expression.LessThan(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildDateLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateTime));
        ConstantExpression DateValueExpression = Expression.Constant(filter.Date.AddDays(1), typeof(DateTime));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }    
    
    private static Expression BuildDateOnlyLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(DateOnly));
        ConstantExpression DateValueExpression = Expression.Constant(DateOnly.FromDateTime(filter.Date), typeof(DateOnly));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }
    private static Expression BuildTimeOnlyLessThanOrEqualCondition(Expression property, FilterRequestDto filter)
    {
        Expression DateNonNullableProperty = Expression.Convert(property, typeof(TimeOnly));
        ConstantExpression DateValueExpression = Expression.Constant(TimeOnly.FromTimeSpan(filter.Time.Value), typeof(TimeOnly));
        return Expression.LessThanOrEqual(DateNonNullableProperty, DateValueExpression);
    }

    private static Expression BuildStringCondition(Expression property, FilterRequestDto filter, ConstantExpression value)
    {
        if (string.IsNullOrWhiteSpace(filter.FilterOperator))
        {
            return Expression.Equal(property, value);
        }
        MethodInfo? toLowerMethodInfo;
        MethodCallExpression propertyToLower;
        ToLowerMethod(property, out toLowerMethodInfo, out propertyToLower);
        //Where(_ => sortings.Contains(_.PlateNumber));
        if (filter.FilterValues.Count>0)
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