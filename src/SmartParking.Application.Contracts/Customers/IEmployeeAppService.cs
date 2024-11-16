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

public interface IEmployeeAppService : ICrudAppService<CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>
{
    /// <summary>
    /// Method to add employee to database
    /// It depends on UserManager to add identity user to User Table
    /// User Manager confirms that user has SecurityStamp
    /// Calls repository to add employee to databas
    /// </summary>
    /// <param name="input">employee create dto</param>
    /// <returns>A dto for created employee</returns>
    /// <exception cref="UserFriendlyException">Throws exception when error occurs while saving employee</exception>
    Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);

    /// <summary>
    /// Method  to add employee for Mdkhl Admin to database
    /// It depends on UserManager to add identity user to User Table
    /// User Manager confirms that user has SecurityStamp
    /// Calls repository to add employee to databas
    /// </summary>
    /// <param name="input">employee create dto</param>
    /// <returns>A dto for created employee</returns>
    /// <exception cref="UserFriendlyException">Throws exception when error occurs while saving employee</exception>
    Task<CustomerDto> CreateMdkhlAsync(CreateUpdateCustomerDto input,int organizationId);

    /// <summary>
    /// Method used to get all employees
    /// </summary>
    /// <param name="input">Input used for pagination</param>
    /// <returns>Paged Result of employees</returns>
    Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<PagedResultDto<CustomerDto>> GetListMdkhlAsync(PagedAndSortedResultRequestDto input);
    /// <summary>
    /// Method used to get employee by id 
    /// </summary>
    /// <param name="customerId">id f specific employee</param>
    /// <returns>employeeDto</returns>
    Task<CustomerDto> GetAsync(Guid customerId);
    /// <summary>
	/// abstract method to search of employees
	/// </summary>
	/// <param name="input">specifies max number of employee and if it sorted or not and search term</param>
	/// <returns>employee Dto</returns>
	public Task<PagedResultDto<CustomerDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to search of employees for MDKHL
    /// </summary>
    /// <param name="input">specifies max number of employee and if it sorted or not and search term</param>
    /// <returns>employee Dto</returns>
    public Task<PagedResultDto<CustomerDto>> SearchMDKHL(PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// abstract method to filter of employees
    /// </summary>
    /// <param name="input">specifies max number of employees and if it sorted or not and filter</param>
    /// <returns>employee Dto</returns>
    public Task<PagedResultDto<CustomerDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    /// <summary>
	/// abstract method to filter of employees
	/// </summary>
	/// <param name="input">specifies max number of employees and if it sorted or not and filter</param>
	/// <returns>employee Dto</returns>
	public Task<PagedResultDto<CustomerDto>> FilterMDKHL(PagedAndSortedAndFilteredResultRequestDto input);
    //Task<bool> SendOtpAsync(string mobileNumber);

    /// <summary>
    /// Method used to get vehicles for specific employee
    /// </summary>
    /// <param name="customerId">Customer Id</param>
    /// <returns>list of vehichles</returns>
    Task<List<VehicleDto>> GetVehiclesRelatedToSpecificEmployee(Guid employeeId);
}
