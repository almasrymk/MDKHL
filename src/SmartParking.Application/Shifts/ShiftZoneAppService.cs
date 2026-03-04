using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Filters;
using SmartParking.Garages;
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
    public override async Task<ShiftZoneDto> UpdateAsync(Guid id, CreateUpdateShiftZoneDto dto)
    {
        var garage = await _garageAppService.GetAsync(dto.ZoneId);
        if (garage == null) throw new Exception("Garage not found");

        var shift = await _shiftAppService.GetShiftAsync(dto.ShiftId);
        if (shift == null) throw new Exception("Shift not found");

        dto.ZoneArName = garage.NameAr;
        dto.ZoneEnName = garage.NameEn;
        dto.StartDate = shift.StartDate;
        dto.EndDate = shift.EndDate;
        dto.StartTime = shift.StartTime;
        dto.EndTime = shift.EndTime;
        dto.IsFullDay = shift.IsFullDay;
        dto.ShiftEnglishName = shift.EnglishName;
        dto.ShiftArabicName = shift.ArabicName;

        return await base.UpdateAsync(id, dto);
    }
    /// <inheritdoc/>
    public override async Task<ShiftZoneDto> CreateAsync(CreateUpdateShiftZoneDto dto)
    {
        var shift = await _shiftAppService.GetShiftAsync(dto.ShiftId);
        if (shift == null) throw new Exception("Shift not found");

        var garage = await _garageAppService.GetAsync(dto.ZoneId);
        if (garage == null) throw new Exception("Garage not found");

        dto.ZoneArName = garage.NameAr;
        dto.ZoneEnName = garage.NameEn;
        dto.StartDate = shift.StartDate;
        dto.EndDate = shift.EndDate;
        dto.StartTime = shift.StartTime;
        dto.EndTime = shift.EndTime;
        dto.IsFullDay = shift.IsFullDay;
        dto.ShiftEnglishName = shift.EnglishName;
        dto.ShiftArabicName = shift.ArabicName;

        return await base.CreateAsync(dto);
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
    private async Task<ShiftZoneDto> MapToGetOutputDtoInternalAsync(ShiftZone entity)
    {
        var query = await _repository.WithDetailsAsync(
            x => x.ShiftDetails
        );

        entity = await query.FirstOrDefaultAsync(e => e.Id == entity.Id);

        if (entity == null)
            throw new Exception($"ShiftZone with id {entity.Id} not found");

        var dto = ObjectMapper.Map<ShiftZone, ShiftZoneDto>(entity);

        // Map OrganizationId
        dto.OrganizationId = entity.OrganizationId;

        var garage = await _garageAppService.GetAsync(entity.ZoneId);
        dto.ZoneEnName = garage.NameEn;
        dto.ZoneArName = garage.NameAr;

        return dto;
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

    private async Task<ShiftZoneDto> GetInternalAsync(Guid id, bool forOrganization)
    {
        var queryable = await _repository.WithDetailsAsync(
            x => x.ShiftDetails
        );

        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));

        if (entity == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(ShiftZone), id);
        }

        return forOrganization
            ? await MapToGetOutputDtoForOrganizationAsync(entity)
            : await MapToGetOutputDtoInternalAsync(entity);
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

    private async Task<PagedResultDto<ShiftZoneDto>> FilterInternalAsync(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        var filterExpression = BuildExpression<ShiftZone>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }

        if (!input.Sorting.IsNullOrEmpty())
        {
            queryable = input.Sorting.Contains("DESC")
                ? queryable.OrderBy(input.Sorting.Split(' ')[0] + " descending")
                : queryable.OrderBy(input.Sorting);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(queryable);

        var itemsDto = ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(items);

        return new PagedResultDto<ShiftZoneDto>(totalCount, itemsDto);
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
    /// <inheritdoc/>
    private async Task<PagedResultDto<ShiftZoneDto>> SearchInternalAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.ShiftEnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.ShiftArabicName.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }

        if (!input.Sorting.IsNullOrEmpty())
        {
            queryable = input.Sorting.Contains("DESC")
                ? queryable.OrderBy(input.Sorting.Split(' ')[0] + " descending")
                : queryable.OrderBy(input.Sorting);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(queryable);

        var itemsDto = ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(items);


        return new PagedResultDto<ShiftZoneDto>(totalCount, itemsDto);
    }
}