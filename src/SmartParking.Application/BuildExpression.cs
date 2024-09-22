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
        // Create a parameter for the T object (x => ...)
        ParameterExpression Param = Expression.Parameter(typeof(T), "x");

        // Start with an initial expression (true)
        Expression combinedExpression = Expression.Constant(true);

        // Iterate through each filter in the dictionary

        foreach (var filter in filterDatas)
        {
            if (filter.FilterProperty.IsNullOrEmpty() && filter.FilterOperator.IsNullOrEmpty())
                continue;
            // Get the property of the Hotel object (x.Id, x.Name, x.Location)
            MemberExpression property = Expression.Property(Param, filter.FilterProperty);
            // Create a constant expression with the value to compare to
            ConstantExpression value = Expression.Constant(filter.FilterValue);

            // Create the equality expression (x.Property == value)
            MethodInfo ContainsMethod = typeof(string).GetMethod(filter.FilterOperator, new[] { typeof(string) });
            Expression condition;
            if (filter.IsNot)
            {
                if (filter.FilterOperator == "IsNullOrEmpty")
                {
                    condition = Expression.Not(Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property));

                }
                else
                {
                    condition = Expression.Not(Expression.Call(property, ContainsMethod, value));
                }
            }
            else
            {


                if (filter.FilterOperator == "IsNullOrEmpty")
                {
                    condition = Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, property);

                }
                else
                {
                    condition = Expression.Call(property, ContainsMethod, value);
                }
            }



            // Combine the new condition with the existing combined expression using AND logic
            combinedExpression = Expression.AndAlso(combinedExpression, condition);
        }

        // Create the final lambda expression (x => ...)
        Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(combinedExpression, Param);

        // Output the final expression
        Console.WriteLine(expression);  // Output: x => (((True AndAlso (x.Id == "5")) AndAlso (x.Name == "Luxury Hotel")) AndAlso (x.Location == "Paris"))
        return expression;
    }
}
