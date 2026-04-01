using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Units;
using SmartParking.Enums;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
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
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        input.TenantId = CurrentTenant.Id;
        if (organization is not null)
            input.OrganizationId = organization.Id;
        else
            input.OrganizationId = 0;
        return await base.CreateAsync(input);
    }

    public async Task<List<UnitDto>> GetAllActiveUnitsAsync()
    {
        var query = await _repository.GetQueryableAsync();

        query = query.Where(u => u.Status == Status.Active);

        var result = await query.ToListAsync();


        return ObjectMapper.Map<List<Unit>, List<UnitDto>>(result);
    }
}
