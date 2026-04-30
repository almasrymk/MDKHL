using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Units;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Organizations;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Units;

public class UnitAppService : CrudAppService<
    Unit,
    UnitDto,
    int,
    PagedAndSortedResultRequestDto,
    CreateUpdateUnitDto>,
    IUnitAppService
{
    private readonly IRepository<Unit, int> _repository;
    private readonly IOrganizationsAppService _organizationsAppService;

    public UnitAppService(IRepository<Unit, int> repository,
        IOrganizationsAppService organizationsAppService) : base(repository)
    {
        _repository = repository;
        _organizationsAppService = organizationsAppService;
    }
    public override async Task<UnitDto> CreateAsync(CreateUpdateUnitDto input)
    {
        var query = await _repository.GetQueryableAsync();
        var unit = await query.Where(x => x.Code == input.Code 
        && x.OrganizationId == input.OrganizationId 
        && x.TenantId == CurrentTenant.Id)
            .FirstOrDefaultAsync();
        if (unit != null) 
        {
            throw new Exception("There is already a unit with this code");    
        }
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        input.TenantId = CurrentTenant.Id;
        if (organization is not null)
        {
            input.OrganizationId = organization.Id;
            if (!organization.OrganizationSettings.IsSlotsEditable)
            {
                input.OwnerSlots = organization.OrganizationSettings.OwnerSlots;
                input.RenterSlots = organization.OrganizationSettings.RentalSlots;
            }
        }
        else
            input.OrganizationId = 0;
        return await base.CreateAsync(input);
    }

    public override async Task<UnitDto> UpdateAsync(int id, CreateUpdateUnitDto input)
    {
        
        var result = await GetAsync(id);
        if(result.Code != input.Code)
        {
            var query = await _repository.GetQueryableAsync();
            var unit = await query.Where(x => x.Code == input.Code
            && x.OrganizationId == input.OrganizationId
            && x.TenantId == CurrentTenant.Id)
                .FirstOrDefaultAsync();
            if (unit != null)
            {
                throw new Exception("There is already a unit with this code");
            }
        }
        
        return await base.UpdateAsync(id, input);
    }

    public async Task<List<UnitDto>> GetAllActiveUnitsAsync()
    {
        var query = await _repository.GetQueryableAsync();

        query = query.Where(u => u.Status == Status.Active);

        var result = await query.ToListAsync();


        return ObjectMapper.Map<List<Unit>, List<UnitDto>>(result);
    }

    public async Task<Result<UnitDto>> GetActiveUnitByCodeAsync(string unitCode)
    {
        var query = await _repository.GetQueryableAsync();
        var unit = await query.FirstOrDefaultAsync(u => u.Code == unitCode && u.Status == Status.Active);
        if (unit == null)
        {
            return Result<UnitDto>.Error($"Unit with code '{unitCode}' not found.");
        }
        return Result<UnitDto>.Success(ObjectMapper.Map<Unit, UnitDto>(unit));
    }

    public async Task<PagedResultDto<UnitDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(u =>
                u.Code.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                u.Description.ToLower().Contains(input.SearchingTerm.ToLower()));
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        var units = await AsyncExecuter.ToListAsync(queryable);
        var unitsDto = ObjectMapper.Map<List<Unit>, List<UnitDto>>(units);

        return new PagedResultDto<UnitDto>(totalCount, unitsDto);
    }

    public async Task<PagedResultDto<UnitDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        var filterExpression = BuildExpression<Unit>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        var units = await AsyncExecuter.ToListAsync(queryable);
        var unitsDto = ObjectMapper.Map<List<Unit>, List<UnitDto>>(units);

        return new PagedResultDto<UnitDto>(totalCount, unitsDto);
    }
}
