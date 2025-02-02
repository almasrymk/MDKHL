using FluentValidation.Validators;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        ConstantExpression valueNumber = Expression.Constant(int.Parse(filter.FilterValue));
        return Expression.Equal(property, valueNumber);
    }

    private static Expression BuildDateCondition(Expression property, FilterRequestDto filter)
    {
        switch (filter.FilterOperator)
        {
            case "GreaterThan":
                return BuildDateGreaterThanCondition(property, filter);
            case "LessThan":
                return BuildDateLessThanCondition(property, filter);
            default:
                return BuildDateBetweenCondition(property, filter);
        }
    }

    private static Expression BuildDateBetweenCondition(Expression property, FilterRequestDto filter)
    {
        ConstantExpression startDate = Expression.Constant(filter.StartDate);
        ConstantExpression endDate = Expression.Constant(filter.EndDate);
        var greaterThan = Expression.GreaterThanOrEqual(property, startDate);
        var lessThan = Expression.LessThanOrEqual(property, endDate);
        return Expression.AndAlso(greaterThan, lessThan);
    }
    
    private static Expression BuildDateGreaterThanCondition(Expression property, FilterRequestDto filter)
    {
        ConstantExpression date = Expression.Constant(filter.Date);
        return Expression.GreaterThanOrEqual(property, date);
    }    
    private static Expression BuildDateLessThanCondition(Expression property, FilterRequestDto filter)
    {
        ConstantExpression date = Expression.Constant(filter.Date);
        return Expression.LessThanOrEqual(property, date);
    }

    private static Expression BuildStringCondition(Expression property, FilterRequestDto filter, ConstantExpression value)
    {
        
        if (filter.FilterOperator.IsNullOrWhiteSpace())
        {
            return Expression.Equal(property, value);
        }
        MethodInfo containsMethod = typeof(string).GetMethod(filter.FilterOperator, new[] { typeof(string) });
        Expression condition;

        if (filter.IsNot)
        {
            condition = filter.FilterOperator == "IsNullOrEmpty"
                ? Expression.Not(Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property))
                : Expression.Not(Expression.Call(property, containsMethod, value));
        }
        else
        {
            condition = filter.FilterOperator == "IsNullOrEmpty"
                ? Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property)
                : Expression.Call(property, containsMethod, value);
        }

        return condition;
    }
}