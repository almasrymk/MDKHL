using SmartParking.Entities.Shifts;
using SmartParking.Filters;
using System.Collections.Generic;
using System.Linq;

namespace SmartParking.Shifts.Extensions;

public static class ShiftZoneFilterExtensions
{
    /// <summary>
    /// Applies dynamic filter expressions built from filter descriptors.
    /// </summary>
    public static IQueryable<ShiftZone> ApplyFilters(
        this IQueryable<ShiftZone> queryable, List<FilterRequestDto> filters)
    {
        var filterExpression = BuildExpression<ShiftZone>.BuildFilterExpression(filters);

        return filterExpression is not null
            ? queryable.Where(filterExpression)
            : queryable;
    }
}