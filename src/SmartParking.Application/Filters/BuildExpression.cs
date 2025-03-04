using FluentValidation.Validators;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Filters;

public static class BuildExpression<T>
{
    private static readonly FilterStrategy _filterStrategy;
    static BuildExpression()
    {
        var builders = new List<ConditionBuilder>
        {
            new NumberConditionBuilder(),
            new StringConditionBuilder(),
            new DateConditionBuilder(),
            new EnumConditionBuilder(),
            new DateOnlyConditionBuilder(),
            new TimeOnlyConditionBuilder()
        };

        _filterStrategy = new FilterStrategy(builders);
    }

    public static Expression<Func<T, bool>> BuildFilterExpression(List<FilterRequestDto> filterDatas)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        Expression combinedExpression = Expression.Constant(true);

        foreach (var filter in filterDatas)
        {
            if (string.IsNullOrEmpty(filter.FilterProperty) && string.IsNullOrEmpty(filter.FilterOperator))
                continue;

            Expression property = GetPropertyExpression(param, filter.FilterProperty);
            
            Expression condition = _filterStrategy.GetCondition(property, filter);
            combinedExpression = Expression.AndAlso(combinedExpression, condition);
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

  

    


   
}