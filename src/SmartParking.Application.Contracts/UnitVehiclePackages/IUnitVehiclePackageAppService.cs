using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.UnitVehiclePackages;

public interface IUnitVehiclePackageAppService : ICreateAppService<UnitVehiclePackageDto,CreateUpdateUnitVehiclePackageDto>, 
    IReadOnlyAppService<UnitVehiclePackageDto, Guid,PagedAndSortedResultRequestDto>
{
}
