using SmartParking.CustomerPackages.Dtos;
using SmartParking.Results;
using SmartParking.UnitPackages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.CustomerUnitPackages;

public interface ICustomerUnitPackageAppService :
    IDeleteAppService<int>,
    IReadOnlyAppService<CustomerUnitPackageDto, int>,
    IUpdateAppService<CustomerUnitPackageDto, int, CreateUpdateCustomerUnitPackageDto>
{
    /// <summary>
    /// Retrieves a list of all packages currently assigned to the authenticated customer.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="Result{T}"/> with a list of <see cref="CustomerPackageDto"/> assigned to the customer.
    /// </returns>
    Task<Result<List<CustomerUnitPackageDto>>> GetListOfUnitPackageByCustomer(Guid customerId, int organizationId);
    /// <summary>
    /// Get all customer unit packages by customer id
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<CustomerUnitPackageDto>> GetAllByCustomerId(Guid customerId, PagedAndSortedResultRequestDto input, bool includeDeleted);



    Task<Result<CustomerUnitPackageDto>> AssignPackageToCustomerAsync(UnitPackageDto packageDto, Guid customerId , CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto);

    Task<bool> IfCustomerHasMultiplePackageAsync(string UnitCode, UnitPackageDto dto);
    Task<bool> CheckCustomerUnitPackageVehicleLimitations(Guid CustomerId, int numberOfVehicles);
    /// <summary>
    /// Determines whether the specified package is associated to same unit code before or not.
    /// </summary>
    /// <param name="UnitCode">The code identifying the unit to check. Cannot be null or empty.</param>
    Task<bool> CheckPackageOverUnits(string UnitCode);
    /// <summary>
    /// Determines whether the number of renter slots exceeds the allowed limit for the specified unit.
    /// </summary>
    /// <param name="dto">The unit package data containing information about renter slots and related unit details. Cannot be null.</param>
    /// <param name="unitCode">The unique code identifying the unit to check. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the renter slots
    /// exceed the unit's limit; otherwise, <see langword="false"/>.</returns>
    Task<bool> CheckRenterSlotsOverUnit(UnitPackageDto dto, string unitCode);
    /// <summary>
    /// Validates whether a customer is eligible to rent a unit based on the provided package details.
    /// </summary>
    /// <param name="CustomerId">The unique identifier of the customer to validate.</param>
    /// <param name="dto">The package details used to determine rental eligibility for the customer.</param>
    /// 
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
    /// customer is eligible to rent multiple units; otherwise, <see langword="false"/>.</returns>
    Task<bool> MultipuleCustomerRentalValidation(Guid CustomerId, UnitPackageDto dto, string unitCode);

    Task<Result<CustomerUnitPackageDto>> GetCustomerUnitPackageByCustomerIdAsync(Guid id);
    /// <summary>
    /// get all pronight packages for the authenticated customer with pagination and sorting capabilities.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<CustomerUnitPackageDto>> GetAllPronightPackages(PagedAndSortedResultRequestDto input);
}
