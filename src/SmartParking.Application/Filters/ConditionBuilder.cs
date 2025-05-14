using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.Filters;

public abstract class ConditionBuilder : ISingletonDependency
{
    public abstract TypeFilter SupportedType { get; }
    /// <summary>
    /// Builds an expression based on the filter operator to compare a property with a date value.
    /// </summary>
    /// <param name="property">The property expression to be evaluated in the condition.</param>
    /// <param name="filter">The filter request that contains the filter operator and date values.</param>
    /// <returns>An expression representing the condition based on the filter operator.</returns>
    public abstract Expression BuildCondition(Expression property, FilterRequestDto filter);
}
