using SmartParking.Entities.Shifts;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace SmartParking.Extensions;

public static class ShiftZoneQueryExtensions
{
    /// <summary>
    /// Applies dynamic ascending/descending ordering to the queryable.
    /// </summary>
    public static IQueryable<ShiftZone> ApplySorting(
        this IQueryable<ShiftZone> queryable, string sorting)
    {
        if (string.IsNullOrEmpty(sorting))
            return queryable;

        return sorting.Contains("DESC")
            ? queryable.OrderBy(sorting.Split(' ')[0] + " descending")
            : queryable.OrderBy(sorting);
    }

    /// <summary>
    /// Filters by English or Arabic shift name using a search term.
    /// </summary>
    public static IQueryable<ShiftZone> ApplySearch(
        this IQueryable<ShiftZone> queryable, string searchingTerm)
    {
        if (string.IsNullOrWhiteSpace(searchingTerm))
            return queryable;

        var term = searchingTerm.ToLower();
        return queryable.Where(x =>
            x.ShiftEnglishName.ToLower().Contains(term) ||
            x.ShiftArabicName.ToLower().Contains(term));
    }
}