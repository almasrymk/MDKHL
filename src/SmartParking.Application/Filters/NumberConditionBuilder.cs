

using System;
using System.Linq.Expressions;

namespace SmartParking.Filters;

public class NumberConditionBuilder : ConditionBuilder
{
    public override TypeFilter SupportedType => TypeFilter.Number;
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
}
