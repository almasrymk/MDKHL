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
    public abstract Expression BuildCondition(Expression property, FilterRequestDto filter);
}
