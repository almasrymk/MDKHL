using SmartParking.Cameras;
using SmartParking.Entities.Customers;
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
    Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);    

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
    Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<PagedResultDto<CustomerDto>> GetListMdkhlAsync(PagedAndSortedResultRequestDto input);
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
    Task<List<VehicleDto>> GetVehiclesRelatedToSpecificCustomerMdkhl(Guid customerId);    


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
    Task<int> GetCountCustomersCountForOrganization(Guid ?CurrentTenantId);

}
