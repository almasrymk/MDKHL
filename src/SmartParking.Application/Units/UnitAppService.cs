using Microsoft.EntityFrameworkCore;
using SmartParking.CodeGeneration;
using SmartParking.Entities.Units;
using SmartParking.Organizations;
using System;
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
    private readonly ICodeGenerationAppService _codeGenerationAppService;
    private readonly IOrganizationsAppService _organizationsAppService;

    public UnitAppService(IRepository<Unit, int> repository,
        ICodeGenerationAppService codeGenerationAppService,
        IOrganizationsAppService organizationsAppService) : base(repository)
    {
        _repository = repository;
        _codeGenerationAppService = codeGenerationAppService;
        _organizationsAppService = organizationsAppService;
    }
    public override async Task<UnitDto> CreateAsync(CreateUpdateUnitDto input)
    {
        var query = await _repository.GetQueryableAsync();
        var unit = await query.OrderByDescending(x => x.Code).FirstOrDefaultAsync();
        var code = _codeGenerationAppService.CodeGenerationAsync(unit?.Code, CodeGenerationConsts.Unit);
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        input.Code = code;
        input.TenantId = CurrentTenant.Id;
        input.OrganizationId = organization.Id;
        return await base.CreateAsync(input);
    }

    

}
