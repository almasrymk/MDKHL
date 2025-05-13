using SmartParking.Cameras;
using SmartParking.Enums;
using SmartParking.KeeperShifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.DailyShifts;
/// <summary>
/// Application service interface for managing Daily Shifts.
/// </summary>
public interface IDailyShiftAppService :
    ICrudAppService<
        DailyShiftDto,
        int,
        PagedAndSortedAndSearchResultRequestDto,
        CreateUpdateDailyShiftDto
    >
{
    /// <summary>
    /// Creates a new Daily Shift.
    /// </summary>
    /// <param name="dto">The Daily Shift data transfer object to create.</param>
    /// <returns>The created <see cref="DailyShiftDto"/>.</returns>
    Task<DailyShiftDto> CreateAsync(CreateUpdateDailyShiftDto dto);

    /// <summary>
    /// Updates an existing Daily Shift by its ID.
    /// </summary>
    /// <param name="id">The ID of the Daily Shift to update.</param>
    /// <param name="dto">The updated Daily Shift data.</param>
    /// <returns>The updated <see cref="DailyShiftDto"/>.</returns>
    Task<DailyShiftDto> UpdateAsync(int id, CreateUpdateDailyShiftDto dto);

    /// <summary>
    /// Deletes a Daily Shift by its ID.
    /// </summary>
    /// <param name="id">The ID of the Daily Shift to delete.</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Gets a Daily Shift by its ID.
    /// </summary>
    /// <param name="id">The ID of the Daily Shift.</param>
    /// <returns>The corresponding <see cref="DailyShiftDto"/>.</returns>
    Task<DailyShiftDto> GetAsync(int id);

    /// <summary>
    /// Gets a paginated list of Daily Shifts filtered by tenant and user.
    /// </summary>
    /// <param name="input">Pagination, sorting, and search criteria.</param>
    /// <param name="TenantId">Optional tenant identifier for filtering.</param>
    /// <param name="userId">Optional user identifier for filtering.</param>
    /// <returns>A paged result of <see cref="DailyShiftDto"/>.</returns>
    Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsAsync(
        PagedAndSortedAndSearchResultRequestDto input,
        Guid? TenantId,
        Guid? userId
    );

    /// <summary>
    /// Gets a paginated list of Daily Shifts scoped to an organization.
    /// </summary>
    /// <param name="input">Pagination, sorting, and search criteria.</param>
    /// <param name="TenantId">Optional tenant identifier for filtering.</param>
    /// <param name="userId">Optional user identifier for filtering.</param>
    /// <returns>A paged result of <see cref="DailyShiftDto"/> for the organization.</returns>
    Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsOrganizationAsync(
        PagedAndSortedAndSearchResultRequestDto input,
        Guid? TenantId,
        Guid? userId
    );

    /// <summary>
    /// Builds a Daily Shift DTO from keeper shift input data.
    /// </summary>
    /// <param name="input">The keeper shift data input.</param>
    /// <param name="keeperShiftRequestType">The type of shift request.</param>
    /// <returns>A prepared <see cref="CreateUpdateDailyShiftDto"/>.</returns>
    Task<CreateUpdateDailyShiftDto> CreateDailyShiftDto(
        KeeperShiftDto input,
        KeeperShiftRequestType keeperShiftRequestType
    );
}