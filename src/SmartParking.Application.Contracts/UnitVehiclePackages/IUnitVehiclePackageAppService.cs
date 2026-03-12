using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.UnitVehiclePackages;

public interface IUnitVehiclePackageAppService : ICreateAppService<UnitVehiclePackageDto,CreateUpdateUnitVehiclePackageDto>, 
    IReadOnlyAppService<UnitVehiclePackageDto, Guid,PagedAndSortedResultRequestDto>
{
    /// <summary>
    /// Determines whether vehicle with the specified plate number is already associated with a package with the given customer.
    /// </summary>
    /// <param name="CustomerId">The unique identifier of the customer to check for vehicle duplication.</param>
    /// <param name="EnPlateNumber">The English plate number of the vehicle to check for duplication. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if a
    /// duplicate vehicle exists for the customer; otherwise, <see langword="false"/>.</returns>
    Task<bool> CheckDuplicationForVehicle(Guid CustomerId, string EnPlateNumber);
}
