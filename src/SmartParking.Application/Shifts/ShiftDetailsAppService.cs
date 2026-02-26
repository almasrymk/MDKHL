using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Shifts;
using SmartParking.Shifts.ShiftDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Shifts;

public class ShiftDetailsAppService : CrudAppService<ShiftDetails, ShiftDetailsDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftDetailsDto>, IShiftDetailsAppService
{
    private readonly IRepository<ShiftDetails, Guid> _repository;
    private readonly IRepository<ShiftZone, Guid> _shiftZoneRepository;
    private readonly IDataFilter _dataFilter;

    public ShiftDetailsAppService(
        IRepository<ShiftDetails, Guid> repository,
        IRepository<ShiftZone, Guid> shiftZoneRepository,
        IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _shiftZoneRepository = shiftZoneRepository;
        _dataFilter = dataFilter;
    }

    /// <inheritdoc/>
    public async Task<ShiftDetailsDto?> GetAllListAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await GetAllListInternalAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<ShiftDetailsDto?> GetAllListForOrganizationAsync()
    {
        return await GetAllListInternalAsync();
    }

    /// <inheritdoc/>
    public async Task<ShiftDetailsDto?> GetByShiftZoneAsync(Guid shiftZoneId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await GetByShiftZoneInternalAsync(shiftZoneId);
        }
    }

    /// <inheritdoc/>
    public async Task<ShiftDetailsDto?> GetByShiftZoneForOrganizationAsync(Guid shiftZoneId)
    {
        return await GetByShiftZoneInternalAsync(shiftZoneId);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftDetailsDto>> GetPaginatedByShiftZoneAsync(Guid shiftZoneId, PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await GetPaginatedByShiftZoneInternalAsync(shiftZoneId, input);
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftDetailsDto>> GetPaginatedByShiftZoneForOrganizationAsync(Guid shiftZoneId, PagedAndSortedResultRequestDto input)
    {
        return await GetPaginatedByShiftZoneInternalAsync(shiftZoneId, input);
    }

    #region Private Methods

    /// <summary>
    /// Core logic for retrieving all shift details, shared by both MDKHL and Organization contexts.
    /// </summary>
    private async Task<ShiftDetailsDto?> GetAllListInternalAsync()
    {
        var entities = await _repository.GetListAsync();
        var entity = entities.SingleOrDefault();
        if (entity == null)
            return null;

        return ObjectMapper.Map<ShiftDetails, ShiftDetailsDto>(entity);
    }

    /// <summary>
    /// Core logic for retrieving shift details by shift zone ID, shared by both MDKHL and Organization contexts.
    /// </summary>
    private async Task<ShiftDetailsDto?> GetByShiftZoneInternalAsync(Guid shiftZoneId)
    {
        var shiftZone = await _shiftZoneRepository.FirstOrDefaultAsync(z => z.Id == shiftZoneId);
        if (shiftZone == null) return null;

        var queryable = await _repository.GetQueryableAsync();
        var details = await queryable
            .Include(sd => sd.ShiftStaffDetails)
            .Include(sd => sd.ShiftDevices)
            .FirstOrDefaultAsync(sd => sd.ShiftZoneId == shiftZoneId);

        if (details == null) return null;

        return ObjectMapper.Map<ShiftDetails, ShiftDetailsDto>(details);
    }

    /// <summary>
    /// Core logic for paginated retrieval by shift zone ID, shared by both MDKHL and Organization contexts.
    /// </summary>
    private async Task<PagedResultDto<ShiftDetailsDto>> GetPaginatedByShiftZoneInternalAsync(Guid shiftZoneId, PagedAndSortedResultRequestDto input)
    {
        var shiftZone = await _shiftZoneRepository.FirstOrDefaultAsync(z => z.Id == shiftZoneId);
        if (shiftZone == null) return new PagedResultDto<ShiftDetailsDto>();

        var queryable = await _repository.GetQueryableAsync();
        var detailsQuery = queryable
            .Include(sd => sd.ShiftStaffDetails)
            .Include(sd => sd.ShiftDevices)
            .Where(sd => sd.ShiftZoneId == shiftZoneId);

        var totalCount = await detailsQuery.CountAsync();

        var sorting = input.Sorting ?? nameof(ShiftDetails.Id);
        var parts = sorting.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var prop = parts.Length > 0 ? parts[0] : nameof(ShiftDetails.Id);
        var descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

        var orderedQuery = descending
            ? detailsQuery.OrderByDescending(sd => EF.Property<object>(sd, prop))
            : detailsQuery.OrderBy(sd => EF.Property<object>(sd, prop));

        var items = await orderedQuery
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        return new PagedResultDto<ShiftDetailsDto>(totalCount, ObjectMapper.Map<List<ShiftDetails>, List<ShiftDetailsDto>>(items));
    }

    #endregion
}