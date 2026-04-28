using Microsoft.AspNetCore.Mvc;
using SmartParking.Cameras.Watchers.Dispensers;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Organizations;
using SmartParking.Services.Dispenser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Cameras.Dispensers;

public class DispenserAppService : CrudAppService<Dispenser, DispenserDto, int, PagedAndSortedResultRequestDto, CreateUpdateDispenserDto>, IDispenserAppService
{
    private readonly IRepository<Dispenser, int> _repository;
    private readonly IRepository<Organization, int> _orgRepository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IDataFilter _dataFilter;
    private readonly DispenserService _dispenserService;
    public DispenserAppService(IRepository<Dispenser, int> repository, IDataFilter dataFilter, IRepository<Organization, int> orgRepository, IRepository<Garage, int> garageRepository, DispenserService dispenserService) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _orgRepository = orgRepository;
        _garageRepository = garageRepository;
        _dispenserService = dispenserService;
    }

    [Route("GetAllDispensers")]
    public async Task<PagedResultDto<DispenserDto>> GetAllAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        if (CurrentTenant.Id == null)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await GetList(input, tenantId);
            }
        }
        return await GetList(input, null);
    }

    private async Task<PagedResultDto<DispenserDto>> GetList(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        var queryable = await _repository.GetQueryableAsync();
        var orgRepo = await _orgRepository.GetQueryableAsync();
        var garageRepo = await _garageRepository.GetQueryableAsync();
        var dispenserQuery = from dispenser in queryable
                             join _org in orgRepo on dispenser.OrganizationId equals _org.Id
                             join _garage in garageRepo on dispenser.GarageId equals _garage.Id
                             select new DispenserDto
                             {
                                 Id = dispenser.Id,
                                 DeviceId = dispenser.DeviceId,
                                 Name = dispenser.Name,
                                 OrganizationNameAr = _org.ArabicName,
                                 TotalRemainNumber = dispenser.TotalRemainNumber,
                                 CameraType = dispenser.CameraType,
                                 Status = dispenser.Status,
                                 OrganizationNameEn = _org.EnglishName,
                                 GarageNameAr = _garage.NameAr,
                                 GarageNameEn = _garage.NameEn,
                             };

        dispenserQuery = ApplyTenant(tenantId, dispenserQuery);
        dispenserQuery = ApplySearch(input, dispenserQuery);
        dispenserQuery = ApplySort(input, dispenserQuery);
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        List<DispenserDto> dispensers = await ApplyPaged(input, dispenserQuery);

        return new PagedResultDto<DispenserDto>(
            totalCount,
            dispensers
        );
    }

    public override Task<DispenserDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return base.GetAsync(id);
        }
    }

    public async override Task<DispenserDto> UpdateAsync(int id, CreateUpdateDispenserDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
         //   await _dispenserService.DispenserDeviceIDExistsOnUpdate(input.DeviceId, id);
            return await base.UpdateAsync(id, input);
        }
    }

    public override Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return base.DeleteAsync(id);

        }
    }

    private static IQueryable<DispenserDto> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DispenserDto> queryable)
    {
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
        return queryable;
    }


    private async Task<List<DispenserDto>> ApplyPaged(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DispenserDto> queryable)
    {
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var dispensers = await AsyncExecuter.ToListAsync(queryable);
        return dispensers;
    }

    private static IQueryable<DispenserDto> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DispenserDto> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.DeviceId.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.OrganizationNameAr.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.OrganizationNameEn.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.GarageNameAr.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.GarageNameEn.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.Status.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) 
           );
        }
        return queryable;
    }

    private static IQueryable<DispenserDto> ApplyTenant(Guid? tenantId, IQueryable<DispenserDto> queryable)
    {
        if (tenantId.HasValue)
            queryable = queryable.Where(_ => _.TenantId == tenantId);
        return queryable;
    }

    public async override Task<DispenserDto> CreateAsync(CreateUpdateDispenserDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
          //  await _dispenserService.DispenserDeviceIDExistsOnCreate(input.DeviceId);
            return await base.CreateAsync(input);
        }
    }
}
