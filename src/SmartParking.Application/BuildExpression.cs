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
        // Create a parameter for the T object (x => ...)
        ParameterExpression Param = Expression.Parameter(typeof(T), "x");

        // Start with an initial expression (true)
        Expression combinedExpression = Expression.Constant(true);

        // Iterate through each filter in the dictionary

        foreach (var filter in filterDatas)
        {
            if (filter.FilterProperty.IsNullOrEmpty() && filter.FilterOperator.IsNullOrEmpty())
                continue;
            // Start with the parameter (x)
            Expression property = Param;
            
            Expression condition;
            if (filter.FilterProperty.Contains('.'))
            {
                // Split the filter property to handle nested properties (e.g., "Country.ArabicName")
                string[] propertyParts = filter.FilterProperty.Split('.');



                // Iterate through each part of the nested property
                foreach (var part in propertyParts)
                {
                    property = Expression.Property(property, part);
                }
            }
            else
            {
                property = Expression.Property(Param, filter.FilterProperty);
            }

            // Get the property of the Hotel object (x.Id, x.Country.ArabicName, x.Location)
 

            // Create a constant expression with the value to compare to
            ConstantExpression value = Expression.Constant(filter.FilterValue);
            if (filter.TypeFilter == TypeFilter.Enum)
            {
                Type propertyType = property.Type;
                ConstantExpression valueNumber = Expression.Constant(Convert.ChangeType(Enum.Parse(propertyType, filter.FilterValue.ToString()), propertyType));
                condition = Expression.Equal(property, valueNumber);
                combinedExpression = Expression.AndAlso(combinedExpression, condition);
                continue;
            }

                // Create the equality expression (x.Property == value)
                MethodInfo ContainsMethod = typeof(string).GetMethod(filter.FilterOperator, new[] { typeof(string) });

            
            
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
