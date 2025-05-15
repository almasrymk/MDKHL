using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts;
/// <summary>
/// Application service interface for managing shifts.
/// </summary>
public interface IShiftAppService : ICrudAppService<ShiftDto, int, PagedAndSortedResultRequestDto, CreateUpdateShiftDto>
{
    /// <summary>
    /// Creates a new shift specific to the MDKHL (entry) context.
    /// </summary>
    /// <param name="shift">The shift data to create.</param>
    /// <returns>The created shift.</returns>
    Task<ShiftDto> CreateForMDKHLAsync(CreateUpdateShiftDto shift);

    /// <summary>
    /// Creates a new shift.
    /// </summary>
    /// <param name="shift">The shift data to create.</param>
    /// <returns>The created shift.</returns>
    Task<ShiftDto> CreateAsync(CreateUpdateShiftDto shift);

    /// <summary>
    /// Retrieves a paged list of shifts for the current organization.
    /// </summary>
    /// <param name="input">The filtering, sorting, and paging parameters.</param>
    /// <returns>A paged list of shift DTOs.</returns>
    Task<PagedResultDto<ShiftDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Retrieves a paged list of shifts for the MDKHL (entry) context.
    /// </summary>
    /// <param name="input">The filtering, sorting, and paging parameters.</param>
    /// <param name="tenantId">The tenant ID to filter shifts by.</param>
    /// <returns>A paged list of shift DTOs.</returns>
    Task<PagedResultDto<ShiftDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Updates a shift by its ID.
    /// </summary>
    /// <param name="id">The ID of the shift to update.</param>
    /// <param name="input">The new data for the shift.</param>
    /// <returns>The updated shift DTO.</returns>
    Task<ShiftDto> UpdateAsync(int id, CreateUpdateShiftDto input);

    /// <summary>
    /// Updates a shift specific to the MDKHL (entry) context.
    /// </summary>
    /// <param name="id">The ID of the shift to update.</param>
    /// <param name="input">The new data for the shift.</param>
    /// <returns>The updated shift DTO.</returns>
    Task<ShiftDto> UpdateMDKHLAsync(int id, CreateUpdateShiftDto input);

    /// <summary>
    /// Retrieves a shift by its ID.
    /// </summary>
    /// <param name="id">The ID of the shift to retrieve.</param>
    /// <returns>The shift DTO.</returns>
    Task<ShiftDto> GetShiftAsync(int id);
}
