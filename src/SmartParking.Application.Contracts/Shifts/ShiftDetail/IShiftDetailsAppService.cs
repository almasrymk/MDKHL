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
    /// Retrieves a single <see cref="ShiftDetailsDto"/> for MDKHL context.
    /// The multi-tenant filter is disabled, allowing cross-tenant access.
    /// </summary>
    /// <returns>A <see cref="ShiftDetailsDto"/> if any shift details exist; otherwise <c>null</c>.</returns>
    Task<ShiftDetailsDto?> GetAllListAsync();

    /// <summary>
    /// Retrieves a single <see cref="ShiftDetailsDto"/> for Organization context.
    /// The multi-tenant filter remains active, so results are scoped to the current tenant.
    /// </summary>
    /// <returns>A <see cref="ShiftDetailsDto"/> if any shift details exist within the tenant; otherwise <c>null</c>.</returns>
    Task<ShiftDetailsDto?> GetAllListForOrganizationAsync();

    /// <summary>
    /// Retrieves shift details for the specified shift zone for MDKHL context.
    /// The multi-tenant filter is disabled, allowing cross-tenant access.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone.</param>
    /// <returns>A <see cref="ShiftDetailsDto"/> if found; otherwise <c>null</c>.</returns>
    Task<ShiftDetailsDto?> GetByShiftZoneAsync(Guid shiftZoneId);

    /// <summary>
    /// Retrieves shift details for the specified shift zone for Organization context.
    /// The multi-tenant filter remains active, so results are scoped to the current tenant.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone.</param>
    /// <returns>A <see cref="ShiftDetailsDto"/> if found within the tenant; otherwise <c>null</c>.</returns>
    Task<ShiftDetailsDto?> GetByShiftZoneForOrganizationAsync(Guid shiftZoneId);

    /// <summary>
    /// Retrieves a paginated list of shift details for the specified shift zone for MDKHL context.
    /// The multi-tenant filter is disabled, allowing cross-tenant access.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone to filter results by.</param>
    /// <param name="input">Paging and sorting parameters.</param>
    /// <returns>A <see cref="PagedResultDto{ShiftDetailsDto}"/> containing the paginated results.</returns>
    Task<PagedResultDto<ShiftDetailsDto>> GetPaginatedByShiftZoneAsync(Guid shiftZoneId, PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Retrieves a paginated list of shift details for the specified shift zone for Organization context.
    /// The multi-tenant filter remains active, so results are scoped to the current tenant.
    /// </summary>
    /// <param name="shiftZoneId">The unique identifier of the shift zone to filter results by.</param>
    /// <param name="input">Paging and sorting parameters.</param>
    /// <returns>A <see cref="PagedResultDto{ShiftDetailsDto}"/> containing the paginated tenant-scoped results.</returns>
    Task<PagedResultDto<ShiftDetailsDto>> GetPaginatedByShiftZoneForOrganizationAsync(Guid shiftZoneId, PagedAndSortedResultRequestDto input);
}