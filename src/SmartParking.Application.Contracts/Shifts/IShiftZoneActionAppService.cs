using SmartParking.Results;
using SmartParking.UserShifts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Shifts;

/// <summary>
/// Application service that exposes actions related to shift zones.
/// Provides operations for retrieving shift zone devices data and users assigned to shift zones.
/// </summary>
public interface IShiftZoneActionAppService
{
    /// <summary>
    /// Retrieves the devices data associated with shift zone watchers.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ShiftWatcherActionDto"/> containing the devices data used by shift watchers.
    /// </returns>
    Task<Result<ICollection<ShiftWatcherActionDto>>> GetShiftZoneDevicesDataAsync();

    /// <summary>
    /// Retrieves users assigned to shift zones using paging, sorting and search parameters.
    /// </summary>
    /// <param name="tenantId">An optional tenant identifier to filter users by tenant. Pass <c>null</c> to include all tenants.</param>
    /// <param name="input">Paging, sorting and search input parameters.</param>
    /// <returns>
    /// A <see cref="PagedResultDto{ShiftZoneUserViewDto}"/> containing the paged list of users assigned to shift zones.
    /// </returns>
    Task<PagedResultDto<ShiftZoneUserViewDto>> GetShiftZoneUsersAsync(Guid? tenantId, PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// Return either true or false based on the current user's active shift zone status.
    /// </summary>
    /// <returns>A <see cref="Result{Boolean}"/> indicating whether the current user has an active shift zone.</returns>
    Task<Result<bool>> GetCurrentUserActiveShiftZoneStatusAsync();
}
