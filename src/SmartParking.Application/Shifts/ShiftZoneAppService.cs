using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Shifts;
using SmartParking.Extensions;
using SmartParking.Garages;
using SmartParking.Shifts.Extensions;
using SmartParking.Shifts.ShiftZones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Shifts;

public class ShiftZoneAppService :
    CrudAppService<ShiftZone, ShiftZoneDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftZoneDto>,
    IShiftZoneAppService
{
    private readonly IRepository<ShiftZone, Guid> _repository;
    private readonly IGarageAppService _garageAppService;
    private readonly IShiftAppService _shiftAppService;
    private readonly IDataFilter _dataFilter;

    public ShiftZoneAppService(
        IRepository<ShiftZone, Guid> repository,
        IShiftAppService shiftAppService,
        IGarageAppService garageAppService,
        IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _shiftAppService = shiftAppService;
        _garageAppService = garageAppService;
        _dataFilter = dataFilter;
    }

    /// <inheritdoc/>
    public override async Task<ShiftZoneDto> CreateAsync(CreateUpdateShiftZoneDto dto)
    {
        var shift = await _shiftAppService.GetShiftAsync(dto.ShiftId)
            ?? throw new Exception("Shift not found");

        var garage = await _garageAppService.GetAsync(dto.ZoneId)
            ?? throw new Exception("Garage not found");

        dto.ShiftAndGarageAssignmentDto(shift, garage);

        return await base.CreateAsync(dto);
    }

    /// <inheritdoc/>
    public override async Task<ShiftZoneDto> UpdateAsync(Guid id, CreateUpdateShiftZoneDto dto)
    {
        var garage = await _garageAppService.GetAsync(dto.ZoneId)
            ?? throw new Exception("Garage not found");

        var shift = await _shiftAppService.GetShiftAsync(dto.ShiftId)
            ?? throw new Exception("Shift not found");

        dto.ShiftAndGarageAssignmentDto(shift, garage);

        return await base.UpdateAsync(id, dto);
    }

    /// <inheritdoc/>
    public override async Task<ShiftZoneDto> GetAsync(Guid id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await GetInternalAsync(id, forOrganization: false);
        }
    }

    /// <inheritdoc/>
    public async Task<ShiftZoneDto> GetForOrganizationAsync(Guid id)
    {
        return await GetInternalAsync(id, forOrganization: true);
    }

    /// <inheritdoc/>
    protected override async Task<ShiftZoneDto> MapToGetOutputDtoAsync(ShiftZone entity)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await MapToGetOutputDtoInternalAsync(entity);
        }
    }

    /// <inheritdoc/>
    protected async Task<ShiftZoneDto> MapToGetOutputDtoForOrganizationAsync(ShiftZone entity)
    {
        return await MapToGetOutputDtoInternalAsync(entity);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await FilterInternalAsync(input);
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        return await FilterInternalAsync(input);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await SearchInternalAsync(input);
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        return await SearchInternalAsync(input);
    }

    #region Private Methods
    /// <summary>
    /// Gets a ShiftZone by id, with an option to include organization details in the result.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="forOrganization"></param>
    /// <returns></returns>
    /// <exception cref="Volo.Abp.Domain.Entities.EntityNotFoundException"></exception>
    private async Task<ShiftZoneDto> GetInternalAsync(Guid id, bool forOrganization)
    {
        var queryable = await _repository.WithDetailsAsync(x => x.ShiftDetails);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));

        if (entity == null)
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(ShiftZone), id);

        return forOrganization
            ? await MapToGetOutputDtoForOrganizationAsync(entity)
            : await MapToGetOutputDtoInternalAsync(entity);
    }
    /// <summary>
    /// Map a ShiftZone entity to a ShiftZoneDto, including related details and garage information.
    /// This method is used for both regular and organization-specific retrievals,
    /// with the caller determining which details to include based on the context.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<ShiftZoneDto> MapToGetOutputDtoInternalAsync(ShiftZone entity)
    {
        var query = await _repository.WithDetailsAsync(x => x.ShiftDetails);

        entity = await query.FirstOrDefaultAsync(e => e.Id == entity.Id)
            ?? throw new Exception($"ShiftZone with id {entity.Id} not found");

        var dto = ObjectMapper.Map<ShiftZone, ShiftZoneDto>(entity);

        dto.OrganizationId = entity.OrganizationId;

        var garage = await _garageAppService.GetAsync(entity.ZoneId);
        dto.ZoneEnName = garage.NameEn;
        dto.ZoneArName = garage.NameAr;

        return dto;
    }
    /// <summary>
    /// Executes pagination and maps results to DTOs.
    /// </summary>
    private async Task<PagedResultDto<ShiftZoneDto>> ToPagedResultAsync(
        IQueryable<ShiftZone> queryable, int skipCount, int maxResultCount)
    {
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(skipCount).Take(maxResultCount));

        return new PagedResultDto<ShiftZoneDto>(
            totalCount,
            ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(items));
    }
    /// <summary>
    /// Filters shift zones based on dynamic filter expressions and sorting, with pagination support.
    /// </summary>
    private async Task<PagedResultDto<ShiftZoneDto>> FilterInternalAsync(
        PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = (await _repository.GetQueryableAsync())
            .ApplyFilters(input.filters)   
            .ApplySorting(input.Sorting);

        return await ToPagedResultAsync(queryable, input.SkipCount, input.MaxResultCount);
    }
    /// <summary>
    /// Searches shift zones by English or Arabic name, with sorting and pagination support.
    /// </summary>
    private async Task<PagedResultDto<ShiftZoneDto>> SearchInternalAsync(
        PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = (await _repository.GetQueryableAsync())
            .ApplySearch(input.SearchingTerm)
            .ApplySorting(input.Sorting);

        return await ToPagedResultAsync(queryable, input.SkipCount, input.MaxResultCount);
    }
    #endregion
}