using SmartParking.Cameras.Watchers;
using SmartParking.Shifts.ShiftZones;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Shifts;

/// <summary>
/// Defines operations for retrieving shift zone devices data for the current user's shift.
/// </summary>
/// <remarks>
/// The returned data is filtered based on date, time, midnight-shift allowance, and status.
/// </remarks>
public interface IShiftZoneActionAppService
{
    /// <summary>
    /// Returns the shift zone devices data assigned to the shift zones of the current user's shift.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ShiftWatcherActionDto"/> items that match the date, time,
    /// allowed midnight-shift, and status filters.
    /// </returns>
    Task<ICollection<ShiftWatcherActionDto>> GetShiftZoneDevicesDataAsync();
}