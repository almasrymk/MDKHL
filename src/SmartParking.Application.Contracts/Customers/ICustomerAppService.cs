using SmartParking.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Cutomers;

public interface ICustomerAppService : ICrudAppService<CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>
{

    /// <summary>
    /// Method to add customer to database
    /// It depends on UserManager to add identity user to User Table
    /// User Manager confirms that user has SecurityStamp
    /// Calls repository to add customer to databas
    /// </summary>
    /// <param name="input">Customer create dto</param>
    /// <returns>A dto for created customer</returns>
    /// <exception cref="UserFriendlyException">Throws exception when error occurs while saving customer</exception>
    //Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);    

    /// <summary>
    /// Method  to add customer for Mdkhl Admin to database
    /// It depends on UserManager to add identity user to User Table
    /// User Manager confirms that user has SecurityStamp
    /// Calls repository to add customer to databas
    /// </summary>
    /// <param name="input">Customer create dto</param>
    /// <returns>A dto for created customer</returns>
    /// <exception cref="UserFriendlyException">Throws exception when error occurs while saving customer</exception>
    Task<CustomerDto> CreateMdkhlAsync(CreateUpdateCustomerDto input);


    /// <summary>
    /// Method used to get all customers
    /// </summary>
    /// <param name="input">Input used for pagination</param>
    /// <returns>Paged Result of Customers</returns>
    Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// Retrieves a paginated list of customers filtered for the MDKHL role context.
    /// </summary>
    /// <param name="input">Pagination, sorting, and search request parameters.</param>
    /// <param name="tenantId">The tenant ID to filter customers by (optional).</param>
    /// <returns>
    /// list of <see cref="CustomerDto"/>.
    /// </returns>
    Task<PagedResultDto<CustomerDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);
    /// <summary>
    /// Method used to get customer by id 
    /// </summary>
    /// <param name="customerId">id f specific customer</param>
    /// <returns>CustomerDto</returns>
    Task<CustomerDto> GetAsync(Guid customerId);
    //Task<bool> SendOtpAsync(string mobileNumber);

    /// <summary>
    /// Method used to get vehicles for specific customer
    /// </summary>
    /// <param name="customerId">Customer Id</param>
    /// <returns>list of vehichles</returns>
    Task<PagedResultDto<VehicleDto>> GetVehiclesRelatedToSpecificCustomerMdkhl(Guid customerId, PagedAndSortedResultRequestDto input);


    /// <summary>
    /// abstract method to search of customers
    /// </summary>
    /// <param name="input">specifies max number of customer and if it sorted or not and search term</param>
    /// <returns>customer Dto</returns>
    public Task<PagedResultDto<CustomerDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to search of customers for MDKHL
    /// </summary>
    /// <param name="input">specifies max number of customer and if it sorted or not and search term</param>
    /// <returns>customer Dto</returns>
    public Task<PagedResultDto<CustomerDto>> SearchMDKHL(PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// abstract method to filter of customers
    /// </summary>
    /// <param name="input">specifies max number of customers and if it sorted or not and filter</param>
    /// <returns>customer Dto</returns>
    public Task<PagedResultDto<CustomerDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    /// <summary>
	/// abstract method to filter of customers
	/// </summary>
	/// <param name="input">specifies max number of customers and if it sorted or not and filter</param>
	/// <returns>customer Dto</returns>
	public Task<PagedResultDto<CustomerDto>> FilterMDKHL(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Retrieves the customers details associated with a specific organization.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="CustomerDto"/> containing the customer details.</returns>
    Task<PagedResultDto<CustomerDto>> GetCustomerByOrganization(PagedAndSortedResultRequestDto input, int organizationId);


    /// <summary>
    /// Method used to get Count of Customers for MDKHL.
    /// </summary>
    /// <returns>Count of Customers</returns>
    Task<int> GetCountCustomersCountForMDKHL();
    /// <summary>
    /// Method used to get Count of Customers for organization.
    /// </summary>
    /// <returns>Count of Customers</returns>
    Task<int> GetCountCustomersCountForOrganization(Guid? CurrentTenantId);
    /// <summary>
    /// Updates a customer entity with the specified ID using the provided input.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="input">The customer data to update.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a result of the updated <see cref="CustomerDto"/>.
    /// </returns>
    Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input);

    /// <summary>
    /// Retrieves a paginated list of customers that belong to a specific customer category status.
    /// </summary>
    /// <param name="input">Pagination, sorting, and search request parameters.</param>
    /// <param name="customerCategory">The status of the customer category to filter by.</param>
    /// <param name="organizationId">The ID of the organization to filter by (optional).</param>
    /// <param name="tenantId">The ID of the tenant to filter by (optional).</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a result of a paginated list of <see cref="CustomerDto"/>.
    /// </returns>
    Task<PagedResultDto<CustomerDto>> GetListForCustomerCategoryAsync(
        PagedAndSortedAndSearchResultRequestDto input,
        CustomerCategoryStatus customerCategory,
        int? organizationId,
        Guid? tenantId);

    /// <summary>
    /// Retrieves a paginated list of customers who do not have any associated package.
    /// </summary>
    /// <param name="input">Pagination, sorting, and search request parameters.</param>
    /// <param name="organizationId">The ID of the organization to filter by (optional).</param>
    /// <param name="tenantId">The ID of the tenant to filter by (optional).</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a result of a paginated list of <see cref="CustomerDto"/>.
    /// </returns>
    Task<PagedResultDto<CustomerDto>> GetListNoPackageAsync(
        PagedAndSortedAndSearchResultRequestDto input,
        int? organizationId,
        Guid? tenantId);

    /// <summary>
    /// Retrieves the total number of customers associated with a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a result of the number of customers in the organization.
    /// </returns>
    Task<int> GetNumberOfCustomerAtOrganizationAsync(int organizationId);
}
