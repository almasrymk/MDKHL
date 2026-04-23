using SmartParking.CustomerPackages.Dtos;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.UnitVehiclePackages;

public interface IUnitVehiclePackageAppService : ICreateAppService<UnitVehiclePackageDto,CreateUpdateUnitVehiclePackageDto>, 
    IReadOnlyAppService<UnitVehiclePackageDto, Guid,PagedAndSortedResultRequestDto>,
    IUpdateAppService<UnitVehiclePackageDto,Guid, CreateUpdateUnitVehiclePackageDto>
{
    /// <summary>
    /// Determines whether vehicle with the specified plate number is already associated with a package with the given customer on a given unit.
    /// </summary>
    /// <param name="CustomerId">The unique identifier of the customer to check for vehicle duplication.</param>
    /// <param name="EnPlateNumber">The English plate number of the vehicle to check for duplication. Cannot be null or empty.</param>
    /// <param name="UnitCode">The code of the unit to check for vehicle duplication. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if a
    /// duplicate vehicle exists for the customer; otherwise, <see langword="false"/>.</returns>
    Task<bool> CheckDuplicationForVehicle(Guid CustomerId, string EnPlateNumber, string unitCode);

    /// <summary>
    /// Retrieves all unit vehicle package information associated with the specified customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose vehicle packages are to be retrieved. Cannot be empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a data transfer object with all
    /// vehicle packages for the specified customer.</returns>
    Task<PagedResultDto<UnitVehiclePackageDto>> GetAllByCustomerId(Guid customerId, PagedAndSortedResultRequestDto input);

    Task DeleteByCustomerUnitPackage(int id);
    /// <summary>
    /// Retrieves the unit vehicle package associated with the specified customer unit package identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer unit package to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unit vehicle package data if
    /// found; otherwise, null.</returns>
    Task<UnitVehiclePackageDto> GetByCustomerUnitPackageId(int id);
    /// <summary>
    /// Retrieves the vehicle package information associated with the specified customer ID and vehicle plate number.
    /// </summary>
    /// <param name="input">An object containing the customer ID and vehicle plate number used to identify the vehicle package. Cannot be
    /// null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the vehicle package information
    /// matching the specified customer ID and plate number, or null if no match is found.</returns>
    Task<UnitVehiclePackageDto> GetByCustmoerIdAndPlateNumber(SearchByPlateAndCustomerIdDto input);
}
