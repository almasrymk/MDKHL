using SmartParking.Cutomers;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Customers;
/// <summary>
/// Interface for employee app service.
/// </summary>
public interface IEmployeeAppService : ICrudAppService<CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>
{
    /// <summary>
    /// Method to add employee to database.
    /// It depends on UserManager to add identity user to User table.
    /// UserManager confirms that user has a SecurityStamp.
    /// Calls repository to add employee to database.
    /// </summary>
    /// <param name="input">Employee creation DTO.</param>
    /// <returns>A DTO for the created employee.</returns>
    /// <exception cref="UserFriendlyException">Thrown when an error occurs while saving the employee.</exception>
    Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);

    /// <summary>
    /// Method to add employee for MDKHL Admin to the database.
    /// It depends on UserManager to add identity user to User table.
    /// UserManager confirms that user has a SecurityStamp.
    /// Calls repository to add employee to database.
    /// </summary>
    /// <param name="input">Employee creation DTO.</param>
    /// <param name="organizationId">Organization ID for which the employee is being created.</param>
    /// <returns>A DTO for the created employee.</returns>
    /// <exception cref="UserFriendlyException">Thrown when an error occurs while saving the employee.</exception>
    Task<CustomerDto> CreateMdkhlAsync(CreateUpdateCustomerDto input, int organizationId);

    /// <summary>
    /// Method used to get all employees.
    /// </summary>
    /// <param name="input">Input used for pagination and sorting.</param>
    /// <returns>Paged result of employees.</returns>
    Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Method used to get all employees for MDKHL.
    /// </summary>
    /// <param name="input">Input used for pagination and sorting.</param>
    /// <returns>Paged result of MDKHL employees.</returns>
    Task<PagedResultDto<CustomerDto>> GetListMdkhlAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Method used to get an employee by ID.
    /// </summary>
    /// <param name="customerId">ID of the specific employee.</param>
    /// <returns>A DTO of the employee.</returns>
    Task<CustomerDto> GetAsync(Guid customerId);

    /// <summary>
    /// Abstract method to search for employees.
    /// </summary>
    /// <param name="input">Specifies the maximum number of employees, sorting, and search term.</param>
    /// <returns>Paged result of employee DTOs.</returns>
    Task<PagedResultDto<CustomerDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to search for employees under MDKHL.
    /// </summary>
    /// <param name="input">Specifies the maximum number of employees, sorting, and search term.</param>
    /// <returns>Paged result of employee DTOs.</returns>
    Task<PagedResultDto<CustomerDto>> SearchMDKHL(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter employees.
    /// </summary>
    /// <param name="input">Specifies the maximum number of employees, sorting, and filter conditions.</param>
    /// <returns>Paged result of filtered employee DTOs.</returns>
    Task<PagedResultDto<CustomerDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to filter MDKHL employees.
    /// </summary>
    /// <param name="input">Specifies the maximum number of employees, sorting, and filter conditions.</param>
    /// <returns>Paged result of filtered employee DTOs.</returns>
    Task<PagedResultDto<CustomerDto>> FilterMDKHL(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Method used to get vehicles assigned to a specific employee.
    /// </summary>
    /// <param name="employeeId">The ID of the employee.</param>
    /// <returns>A list of <see cref="VehicleDto"/> associated with the employee.</returns>
    Task<List<VehicleDto>> GetVehiclesRelatedToSpecificEmployee(Guid employeeId);
}
    


