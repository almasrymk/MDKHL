using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts.ShiftDetail;

/// <summary>
/// Application service contract for managing shift details.
/// Inherits standard CRUD operations from <see cref="ICrudAppService{TEntityDto, TKey, TPagedAndSortedRequest, TCreateUpdate}"/>.
/// </summary>
public interface IShiftDetailsAppService : ICrudAppService<ShiftDetailsDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftDetailsDto>
{
    /// <summary>
    /// Retrieves a <see cref="ShiftDetailsDto"/> that represents all available shift details.
    /// </summary>
    /// <remarks>
    /// Implementation may return a single aggregated result representing all shift zones,
    /// or the first/only available shift details record depending on business logic.
    /// </remarks>
    /// <returns>
    /// A <see cref="ShiftDetailsDto"/> if any shift details exist; otherwise <c>null</c>.
    /// </returns>
    Task<ShiftDetailsDto?> GetAllListAsync();

    /// <summary>
    /// Retrieves the shift details for the specified shift zone.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone to retrieve details for.</param>
    /// <returns>
    /// A <see cref="ShiftDetailsDto"/> for the specified shift zone if found; otherwise <c>null</c>.
    /// </returns>
    Task<ShiftDetailsDto?> GetByShiftZoneAsync(Guid shiftZoneId);

    /// <summary>
    /// Retrieves a paginated list of shift details for the specified shift zone.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone to filter results by.</param>
    /// <param name="input">Paging and sorting parameters.</param>
    /// <returns>
    /// A <see cref="PagedResultDto{ShiftDetailsDto}"/> containing the paginated shift details for the specified shift zone.
    /// </returns>
    Task<PagedResultDto<ShiftDetailsDto>> GetPaginatedByShiftZoneAsync(Guid shiftZoneId, PagedAndSortedResultRequestDto input);
}