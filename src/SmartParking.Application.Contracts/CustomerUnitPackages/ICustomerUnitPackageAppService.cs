using SmartParking.CustomerPackages;
using SmartParking.Results;
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
    Task<Result<List<CustomerUnitPackageDto>>> GetUnitPackageByCustomer(Guid customerId);


    Task<Result<CustomerUnitPackageDto>> AssignPackageToCustomerAsync(int packageId, Guid customerId , CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto);

    Task<bool> IfCustomerHasMultiplePackageAsync(Guid CustomerId);
    Task<bool> CheckCustomerUnitPackageVehicleLimitations(Guid CustomerId, int numberOfVehicles);

}
