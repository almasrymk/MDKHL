using FluentValidation;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Filters;

internal class FilterStrategy
{
    private readonly Dictionary<TypeFilter, ConditionBuilder> _builders;

    public FilterStrategy(IEnumerable<ConditionBuilder> builders)
    {
        _builders = builders.ToDictionary(b => b.SupportedType);
    }

    public Expression GetCondition(Expression property, FilterRequestDto filter)
    {
        if (_builders.TryGetValue(filter.TypeFilter, out var builder))
        {
            return builder.BuildCondition(property, filter);
        }
        throw new Exception(LocalizationHelper.GetLocalizedString("Filter") + " " + filter.TypeFilter + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
    }
}
