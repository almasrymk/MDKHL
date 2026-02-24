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
            var entities = await _repository.GetListAsync();
            var entity = entities.SingleOrDefault();
            if (entity == null)
                return null;
            var dto = ObjectMapper.Map<ShiftDetails, ShiftDetailsDto>(entity);
            return dto;
        }
    }
    /// <inheritdoc/>
    public async Task<ShiftDetailsDto?> GetByShiftZoneAsync(Guid shiftZoneId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // 1. Check if the zone exists
            var shiftZone = await _shiftZoneRepository.FirstOrDefaultAsync(z => z.Id == shiftZoneId);
            if (shiftZone == null) return null;

            // 2. Query ShiftDetails by the relationship 
            // (Assuming ShiftDetails has a ShiftZoneId property or we access the first one in the collection)
            var queryable = await _repository.GetQueryableAsync();

            var details = await queryable
                .Include(sd => sd.ShiftStaffDetails)
                .Include(sd => sd.ShiftDevices)
                // We look for the first detail record associated with this zone
                .FirstOrDefaultAsync(sd => sd.ShiftZoneId == shiftZoneId);

            if (details == null) return null;

            return ObjectMapper.Map<ShiftDetails, ShiftDetailsDto>(details);
        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftDetailsDto>> GetPaginatedByShiftZoneAsync(Guid shiftZoneId, PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
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

            var dtos = ObjectMapper.Map<List<ShiftDetails>, List<ShiftDetailsDto>>(items);
            return new PagedResultDto<ShiftDetailsDto>(totalCount, dtos);
        }
    }
}