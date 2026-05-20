using SmartParking.Entities.UnitPackageIntegrations;
using SmartParking.UnitPackageIntegrations.Dtos;
using System;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.UnitPackageIntegrations;

[RemoteService(false)]
public class UnitPackageIntegrationAppService
    : CrudAppService<UnitPackageIntegration, UnitPackageIntegrationDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateUnitPackageIntegrationDto>,
      IUnitPackageIntegrationAppService
{
    private readonly IRepository<UnitPackageIntegration, Guid> _repository;

    public UnitPackageIntegrationAppService(IRepository<UnitPackageIntegration, Guid> repository)
        : base(repository)
    {
        _repository = repository;
    }
}
