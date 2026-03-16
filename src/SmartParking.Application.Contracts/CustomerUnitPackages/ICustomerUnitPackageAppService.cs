using SmartParking.CustomerPackages.Dtos;
using SmartParking.Results;
using SmartParking.UnitPackages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.CustomerUnitPackages;

public interface ICustomerUnitPackageAppService :
    IDeleteAppService<int>,
    IReadOnlyAppService<CustomerUnitPackageDto, int>
{
    /// <summary>
    /// Retrieves a list of all packages currently assigned to the authenticated customer.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="Result{T}"/> with a list of <see cref="CustomerPackageDto"/> assigned to the customer.
    /// </returns>
    Task<Result<List<CustomerUnitPackageDto>>> GetListOfUnitPackageByCustomer(Guid customerId, int organizationId);



    Task<Result<CustomerUnitPackageDto>> AssignPackageToCustomerAsync(int packageId, Guid customerId , CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto);

    Task<bool> IfCustomerHasMultiplePackageAsync(Guid CustomerId, UnitPackageDto dto);
    Task<bool> CheckCustomerUnitPackageVehicleLimitations(Guid CustomerId, int numberOfVehicles);
    /// <summary>
    /// Determines whether the specified package is associated to same unit code before or not.
    /// </summary>
    /// <param name="UnitCode">The code identifying the unit to check. Cannot be null or empty.</param>
    /// <param name="UnitPackageId">The identifier of the package associated with the unit. Must be a valid package ID.</param>
    Task<bool> CheckPackageOverUnits(string UnitCode, int UnitPackageId);

}
