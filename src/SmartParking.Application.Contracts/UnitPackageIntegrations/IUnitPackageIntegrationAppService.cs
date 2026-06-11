using SmartParking.UnitPackageIntegrations.Dtos;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.UnitPackageIntegrations;

public interface IUnitPackageIntegrationAppService : 
    ICrudAppService<UnitPackageIntegrationDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateUnitPackageIntegrationDto>
{
}
