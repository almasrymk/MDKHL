using Microsoft.EntityFrameworkCore;
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
using Volo.Abp.Domain.Entities;
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
    private readonly IRepository<ShiftDetails, Guid> _shiftDetailsRepository;
    private readonly IRepository<ShiftDevice, Guid> _shiftDeviceRepository;
    private readonly IRepository<ShiftStaffDetails, Guid> _shiftStaffDetailsRepository;
    private readonly IRepository<ShiftZone, Guid> _shiftZoneRepository;


    public ShiftZoneAppService(
        IRepository<ShiftDetails, Guid> shiftDetailsRepository,
        IRepository<ShiftDevice, Guid> shiftDeviceRepository,
        IRepository<ShiftStaffDetails, Guid> shiftStaffDetailsRepository,
        IRepository<ShiftZone, Guid> shiftZoneRepository,
        IRepository<ShiftZone, Guid> repository,
        IShiftAppService shiftAppService,
        IGarageAppService garageAppService,
        IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _shiftAppService = shiftAppService;
        _garageAppService = garageAppService;
        _shiftZoneRepository = shiftZoneRepository;
        _dataFilter = dataFilter;
        _shiftDetailsRepository = shiftDetailsRepository;
        _shiftDeviceRepository = shiftDeviceRepository;
        _shiftStaffDetailsRepository = shiftStaffDetailsRepository;
    }
    public override async Task<ShiftZoneDto> UpdateAsync(Guid id, CreateUpdateShiftZoneDto dto)
    {
        var garage = await _garageAppService.GetAsync(dto.ZoneId);
        if (garage == null) throw new Exception("Garage not found");

        dto.ZoneArName = garage.NameAr;
        dto.ZoneEnName = garage.NameEn;

        return await base.UpdateAsync(id, dto);
    }
    public override async Task<ShiftZoneDto> CreateAsync(CreateUpdateShiftZoneDto dto)
    {
        var shift = await _shiftAppService.GetShiftAsync(dto.ShiftId);
        if (shift == null) throw new Exception("Shift not found");

        var garage = await _garageAppService.GetAsync(dto.ZoneId);
        if (garage == null) throw new Exception("Garage not found");

        dto.ZoneArName = garage.NameAr;
        dto.ZoneEnName = garage.NameEn;

        return await base.CreateAsync(dto);
    }

    protected override async Task<ShiftZoneDto> MapToGetOutputDtoAsync(ShiftZone entity)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.WithDetailsAsync(
            x => x.Shift,
            x => x.ShiftDetails
        );

            entity = await query.FirstOrDefaultAsync(e => e.Id == entity.Id);

            if (entity == null)
                throw new Exception($"ShiftZone with id {entity.Id} not found");

            var dto = ObjectMapper.Map<ShiftZone, ShiftZoneDto>(entity);

            var garage = await _garageAppService.GetAsync(entity.ZoneId);

            dto.ShiftDto.GarageEnglishName = garage.NameEn;
            dto.ShiftDto.GarageArabicName = garage.NameAr;

            return dto;
        }
    }

    protected override async Task<IQueryable<ShiftZone>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .Include(x => x.Shift);
    }

    public override async Task<ShiftZoneDto> GetAsync(Guid id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.WithDetailsAsync(
                x => x.Shift,
                x => x.ShiftDetails
            );

            var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));

            if (entity == null)
            {
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(ShiftZone), id);
            }

            return await MapToGetOutputDtoAsync(entity);
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<subscriptionTypes> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<ShiftZone>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of subscriptionTypes entities
            var subscriptionTypes = await AsyncExecuter.ToListAsync(queryable);

            // Map the subscriptionTypes entities to subscriptionTypesDto objects
            var subscriptionTypesDto = ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(subscriptionTypes);
            // Return the paginated result
            return new PagedResultDto<ShiftZoneDto>(
                totalCount,
                subscriptionTypesDto
            );
        }

    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftZoneDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<garage> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply search filtering
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                    or.Shift.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.Shift.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower())

                );
            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of hotel entities
            var SubscriptionTypes = await AsyncExecuter.ToListAsync(queryable);

            // Map the hotel entities to hotelDto objects
            var SubscriptionTypesDto = ObjectMapper.Map<List<ShiftZone>, List<ShiftZoneDto>>(SubscriptionTypes);
            Console.WriteLine(SubscriptionTypesDto.Count);
            // Return the paginated result
            return new PagedResultDto<ShiftZoneDto>(
                totalCount,
                SubscriptionTypesDto
            );
        }
    }

    public override async Task DeleteAsync(Guid id)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(
            x => x.ShiftDetails!.ShiftDevices,
            x => x.ShiftDetails!.ShiftStaffDetails
        );

        var shiftZone = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.Id == id)
        );

        if (shiftZone == null)
            throw new EntityNotFoundException(typeof(ShiftZone), id);

        if (shiftZone.ShiftDetails != null)
        {
            foreach (var device in shiftZone.ShiftDetails.ShiftDevices ?? new())
                await _shiftDeviceRepository.HardDeleteAsync(device, autoSave: false);

            foreach (var staff in shiftZone.ShiftDetails.ShiftStaffDetails ?? new())
                await _shiftStaffDetailsRepository.HardDeleteAsync(staff, autoSave: false);

            await _shiftDetailsRepository.HardDeleteAsync(
                shiftZone.ShiftDetails, autoSave: false
            );
        }

        await _shiftZoneRepository.HardDeleteAsync(shiftZone, autoSave: true);
    }
}
