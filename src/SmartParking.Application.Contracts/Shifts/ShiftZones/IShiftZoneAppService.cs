using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts.ShiftZones;

public interface IShiftZoneAppService : ICrudAppService<ShiftZoneDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftZoneDto>
{
    /// <summary>
    /// Retrieves a single <see cref="ShiftZoneDto"/> by its ID for Organization context.
    /// The multi-tenant filter remains active, so results are scoped to the current tenant.
    /// </summary>
    /// <param name="id">The unique identifier of the ShiftZone.</param>
    /// <returns>The matching <see cref="ShiftZoneDto"/>.</returns>
    Task<ShiftZoneDto> GetForOrganizationAsync(Guid id);

    /// <summary>
    /// Filters ShiftZones by column expressions for MDKHL context.
    /// The multi-tenant filter is disabled, allowing cross-tenant access.
    /// </summary>
    /// <param name="input">The filter, sorting, and pagination parameters.</param>
    /// <returns>A paged list of <see cref="ShiftZoneDto"/> matching the filter criteria.</returns>
    Task<PagedResultDto<ShiftZoneDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Filters ShiftZones by column expressions for Organization context.
    /// The multi-tenant filter remains active, so results are scoped to the current tenant.
    /// </summary>
    /// <param name="input">The filter, sorting, and pagination parameters.</param>
    /// <returns>A paged list of <see cref="ShiftZoneDto"/> matching the filter criteria.</returns>
    Task<PagedResultDto<ShiftZoneDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Searches ShiftZones by shift name (Arabic or English) for MDKHL context.
    /// The multi-tenant filter is disabled, allowing cross-tenant access.
    /// </summary>
    /// <param name="input">The search term, sorting, and pagination parameters.</param>
    /// <returns>A paged list of <see cref="ShiftZoneDto"/> matching the search term.</returns>
    Task<PagedResultDto<ShiftZoneDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Searches ShiftZones by shift name (Arabic or English) for Organization context.
    /// The multi-tenant filter remains active, so results are scoped to the current tenant.
    /// </summary>
    /// <param name="input">The search term, sorting, and pagination parameters.</param>
    /// <returns>A paged list of <see cref="ShiftZoneDto"/> matching the search term.</returns>
    Task<PagedResultDto<ShiftZoneDto>> Search(PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// Updates the shift zone identified by the specified ID with the provided data.
    /// </summary>
    /// <remarks>Throws an exception if the specified ID does not correspond to an existing shift zone or if
    /// the provided data is invalid.</remarks>
    /// <param name="id">The unique identifier of the shift zone to update.</param>
    /// <param name="dto">An object containing the updated information for the shift zone.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated shift zone data.</returns>
    Task<ShiftZoneDto> UpdateAsync(Guid id, CreateUpdateShiftZoneDto dto);
    /// <summary>
    /// Creates a new shift zone with the provided data.
    /// </summary>
    /// <param name="dto">An object containing the information for the new shift zone.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created shift zone data.</returns>
    Task<ShiftZoneDto> CreateAsync(CreateUpdateShiftZoneDto dto);
    /// <summary>
    /// Retrieves a shift zone by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift zone to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the shift zone data.</returns>
    Task<ShiftZoneDto> GetAsync(Guid id);

}