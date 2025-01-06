using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using SmartParking.Entities.Customers;
using Volo.Abp.Application.Dtos;
using SmartParking.Cutomers;
using System.Linq;
using Volo.Abp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using SmartParking.Entities.Vehicles;
using SmartParking.Vehicles;
using SmartParking.Entities.Organizations;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using SmartParking.Organizations;
using Microsoft.AspNetCore.Authorization;
using Abp.Domain.Entities;
using SmartParking.CustomerPackages;
using SmartParking.Authentication;
using SmartParking.Enums;
using SmartParking.Utilities;
using SmartParking.Localization;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using Microsoft.AspNetCore.Identity;
namespace SmartParking.Customers;
[Authorize]
public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>, ICustomerAppService
{
    private readonly IRepository<Customer, Guid> _repository;
    private readonly IRepository<Vehicle, int> _repositoryVehicle;
    private IOrganizationsAppService _organizationAppService;
    private IDataFilter _dataFilter;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IBaseCustomerAppService _baseCustomer;
	private readonly ICustomerPackagesAppService _packagesAppService;
    private readonly IUsersAppService _usersAppService;
    private readonly IAuthenticationUtility _authenticationUtility;

    public CustomerAppService(IRepository<Customer, Guid> repository, IRepository<Vehicle, int> repositoryVehicle, IDataFilter dataFilter,
        IOrganizationsAppService organizationAppService, UserManager<IdentityUser> userManager,
        IBaseCustomerAppService baseCustomer , ICustomerPackagesAppService packagesAppService,
        IUsersAppService usersAppService, IAuthenticationUtility authenticationUtility) : base(repository)
    {
        _repository = repository;
        _repositoryVehicle = repositoryVehicle;
        _dataFilter = dataFilter;
        _organizationAppService = organizationAppService;
        _userManager = userManager;
        _baseCustomer = baseCustomer;
		_packagesAppService = packagesAppService;
        _usersAppService = usersAppService;
        _authenticationUtility = authenticationUtility;
    }
  

    /// <inheritdoc/>
  //  public async override Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
  //  {
  //      try
		//{
		//	var organization = await _organizationAppService.GetOrganizationForCurrentTenat();
		//	using (_dataFilter.Disable<IMultiTenant>())
                
		//	{
		//		var user = await _userManager.FindByNameAsync(input.UserName); 
		//		if (user != null)
		//		{
  //                  throw new Exception(LocalizationHelper.GetLocalizedString("User") + " " +LocalizationHelper.GetLocalizedString("NotFoundValidation"));
  //              }

  //          }
		//	await _baseCustomer.CheckCustomer(input);

		//	var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
		//	await AddUser(input, customer);
		//	var customerAdd = await _repository.InsertAsync(customer, true);
  //          await _packagesAppService.AssignPackageToCustomerWebAsync(input.PackageId, customerAdd.Id);
			
		//	return ObjectMapper.Map<Customer, CustomerDto>(customer);
		//}
		//catch (Exception ex)
  //      {

  //          throw new UserFriendlyException("Registration failed: " + ex.Message);
  //      }
  //  }

	

	/// <inheritdoc/>
	public async Task<CustomerDto> CreateMdkhlAsync(CreateUpdateCustomerDto input)
	{
		
			using (_dataFilter.Disable<IMultiTenant>())
			{
				var user = await _userManager.FindByNameAsync(input.UserName);
				if (user != null)
				{
                    throw new BusinessException(LocalizationHelper.GetLocalizedString("User") +" "+ 
                        LocalizationHelper.GetLocalizedString("NotFoundValidation"));

                }
			}
			await _baseCustomer.CheckCustomer(input);
            var result1 = await _baseCustomer.CheckIfPhoneNumberIsUnique(input.PhoneNumber);
            if (!result1)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("PhoneNumber") +" "+ 
                    LocalizationHelper.GetLocalizedString("DuplicateValidation"));

            }
            var result2 = await _baseCustomer.CheckIfNationalIDIsUnique(input.NationalId);
            if (!result2)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("NationId") +" "+ 
                    LocalizationHelper.GetLocalizedString("DuplicateValidation"));

            }
            var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
            UserDto userCreated;
            if (input.IsUser)
                userCreated = await AddUser(input);
            else
             userCreated = await AddUser(input, customer);
            customer.IdentityUserId = userCreated.Id;
			var customerAdd = await _repository.InsertAsync(customer, true);
			await _packagesAppService.AssignPackageToCustomerWebAsync(input.PackageId, customerAdd.Id);
			return ObjectMapper.Map<Customer, CustomerDto>(customer);
		

	}


	/// <summary>
	/// Asynchronously adds a user based on the provided customer data.
	/// </summary>
	/// <param name="input">The DTO containing customer details for user creation.</param>
	/// <param name="customer">The customer entity to associate with the new user.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	private async Task<UserDto> AddUser(CreateUpdateCustomerDto input, Customer customer)
	{
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var userMap = ObjectMapper.Map< CreateUpdateCustomerDto , UserCreateDto>(input);
            userMap.TenantId = null;
            var role = await _authenticationUtility.GetRoleByNameAndTenant(RolesType.Customer.ToString(), null);
            var user = await _usersAppService.CreateWithRoleAsync(userMap,role.Id);
            return user;
		}
	}		
    
    private async Task<UserDto> AddUser(CreateUpdateCustomerDto input)
	{
        
            var userMap = ObjectMapper.Map< CreateUpdateCustomerDto , UserCreateDto>(input);
            userMap.TenantId = input.TenantId;
            userMap.IsCustomer = true;
            var user = await _usersAppService.CreateWithRoleAsync(userMap,input.RoleId);
            return user;
	}	
    



	


	/// <inheritdoc/>
	public async override Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedResultRequestDto input)
	{
		 var organization = await _organizationAppService.GetOrganizationForCurrentTenatAsync();
        using (_dataFilter.Disable<IMultiTenant>())
        {


            var query = await _repository.GetQueryableAsync();
            query = query.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
			query = query.Where(x => x.CustomerPackages.Any(org => org.OrganizationId == organization.Id));
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    query = query.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    query = query.OrderBy(input.Sorting);
                }

            }
            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            var customers = await AsyncExecuter.ToListAsync(query);
            var cutomersDtos = ObjectMapper.Map<List<Customer>, List<CustomerDto>>(customers);
            return new PagedResultDto<CustomerDto>
            {
                TotalCount = totalCount,
                Items = cutomersDtos
            };
        }
	}
	/// <inheritdoc/>
	public async Task<PagedResultDto<CustomerDto>> GetListMdkhlAsync(PagedAndSortedResultRequestDto input)
	{
		using (_dataFilter.Disable<IMultiTenant>())
		{

			var query = await _repository.GetQueryableAsync();
			query = query.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    query = query.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    query = query.OrderBy(input.Sorting);
                }

            }
            var totalCount = await AsyncExecuter.CountAsync(query);
			query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
			var customers = await AsyncExecuter.ToListAsync(query);
			var cutomersDtos = ObjectMapper.Map<List<Customer>, List<CustomerDto>>(customers);
            
			return new PagedResultDto<CustomerDto>
			{
				TotalCount = totalCount,
				Items = cutomersDtos
			};

		}

	}

	/// <inheritdoc/>
	public override async Task<CustomerDto> GetAsync(Guid id)
    {
        
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer =  await _repository.GetAsync(c => c.Id == id);
            return ObjectMapper.Map<Customer,CustomerDto>(customer);

        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<CustomerDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var organization = await _organizationAppService.GetOrganizationForCurrentTenatAsync();

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            
            queryable = queryable.Include(x => x.IdentityUser);
            queryable = queryable.Include(c => c.CustomerPackages);
            queryable = queryable.Where(x => x.CustomerPackages.Any(cp => cp.OrganizationId == organization.Id));

            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
                {

                    queryable = queryable.Where(or => 
                        or.IdentityUser.UserName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                        or.IdentityUser.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                        or.NationalId.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                        or.IdentityUser.Email.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                        or.IdentityUser.PhoneNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                        or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower())
                    );
                }
                if (!input.Sorting.IsNullOrEmpty())
                {
                    if (input.Sorting.Contains("DESC"))
                    {
                        var sorting = input.Sorting.Split(' ').ToList();
                        queryable = queryable.OrderBy(sorting[0] + " descending");
                    }
                    else
                    {
                        queryable = queryable.OrderBy(input.Sorting);
                    }

                }
                var totalCount = await AsyncExecuter.CountAsync(queryable);

                queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
                var customers = await AsyncExecuter.ToListAsync(queryable);

                var customersDto = ObjectMapper.Map<List<Customer>, List<CustomerDto>>(customers);
                Console.WriteLine(customersDto.Count);
                return new PagedResultDto<CustomerDto>(
                    totalCount,
                    customersDto
                );
        }

    }
    public async Task<PagedResultDto<CustomerDto>> SearchMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
           
            var queryable = await _repository.GetQueryableAsync();
            queryable = queryable.Include(x => x.IdentityUser);
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                    or.IdentityUser.UserName.ToLower().Contains(input.SearchingTerm.ToLower())||
                    or.IdentityUser.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.NationalId.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.IdentityUser.Email.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.IdentityUser.PhoneNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.IdentityUser.Surname.ToLower().Contains(input.SearchingTerm.ToLower())||
                    or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower())
                );

            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            var customers = await AsyncExecuter.ToListAsync(queryable);

            var customersDto = ObjectMapper.Map<List<Customer>, List<CustomerDto>>(customers);
            Console.WriteLine(customersDto.Count);
            return new PagedResultDto<CustomerDto>(
                totalCount,
                customersDto
            );
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<CustomerDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {

            var orgid = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenant.Id);
            var queryable = await _repository.GetQueryableAsync();

            queryable = queryable.Include(x => x.IdentityUser);
            queryable = queryable.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
            queryable = queryable.Where(x => x.CustomerPackages.Any(org => org.OrganizationId == orgid));


            var filterExpression = BuildExpression<Customer>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of garages entities
            var customers = await AsyncExecuter.ToListAsync(queryable);

            // Map the garages entities to garagesDto objects
            var customersDto = ObjectMapper.Map<List<Customer>, List<CustomerDto>>(customers);
            Console.WriteLine(customersDto.Count);
            // Return the paginated result
            return new PagedResultDto<CustomerDto>(
                totalCount,
                customersDto
            );
        }
    }/// <inheritdoc/>
	public async Task<PagedResultDto<CustomerDto>> FilterMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {

            var queryable = await _repository.GetQueryableAsync();
            var filterExpression = BuildExpression<Customer>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of garages entities
            var customers = await AsyncExecuter.ToListAsync(queryable);

            // Map the garages entities to garagesDto objects
            var customersDto = ObjectMapper.Map<List<Customer>, List<CustomerDto>>(customers);
            Console.WriteLine(customersDto.Count);
            // Return the paginated result
            return new PagedResultDto<CustomerDto>(
                totalCount,
                customersDto
            );
        }
    }

    /// <inheritdoc/>
    public async Task<List<VehicleDto>> GetVehiclesRelatedToSpecificCustomerMdkhl(Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await GetAsync(customerId);
            if (customer == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Customer") + " " +
                    LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }

            var query = await _repositoryVehicle.GetQueryableAsync();

            query = query.Where(x => x.CustomerId == customerId);
            var result = await AsyncExecuter.ToListAsync(query);
            var output = ObjectMapper.Map<List<Vehicle>, List<VehicleDto>>(result);
            return output;
        }

    }


    /// <inheritdoc/>
    public async Task<PagedResultDto<CustomerDto>> GetCustomerByOrganization(PagedAndSortedResultRequestDto input, int organizationId)
    {
        var org = await _organizationAppService.GetAsync(organizationId);
        if (org == null)
        {
            throw new EntityNotFoundException(LocalizationHelper.GetLocalizedString("Organization") + " " +
                      LocalizationHelper.GetLocalizedString("NotFoundValidation"));

        }
        using (_dataFilter.Disable<IMultiTenant>())
        {

        var query = await _repository.GetQueryableAsync();
        query = query.Where(c => c.IsEmployee==false);
        query = query.Where(x => x.CustomerPackages.Any(org => org.OrganizationId == organizationId));
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        query.OrderBy(x => x.Id);
        var customers = await AsyncExecuter.ToListAsync(query);
        var customerDtos = ObjectMapper.Map<List<Customer>, List<CustomerDto>>(customers);
        return new PagedResultDto<CustomerDto>(
           totalCount,
           customerDtos
       );
        }


    }

   

	public async Task<int> GetCountCustomersCountForMDKHL()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => !c.IsEmployee);
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        }
    }


    public async Task<int> GetCountCustomersCountForOrganization(Guid? CurrentTenantId)
    {
        var orgId = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenantId);

        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => !c.IsEmployee);
            query = query.Include(x => x.CustomerPackages).ThenInclude(c => c.Organization).Where(x => x.CustomerPackages.Any(o => o.OrganizationId == orgId));
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        }
    }



    /// <inheritdoc/>

    public async override Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _repository.FirstOrDefaultAsync(c => c.Id == id);
     
            if (customer == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Customer") + " " + 
                    LocalizationHelper.GetLocalizedString("NotFoundValidation"));

            }
            if (customer.IdentificationTypes != input.IdentificationType || customer.NationalId != input.NationalId)
            {
                await _baseCustomer.CheckCustomer(input);
            }
            if (customer.IdentityUser.PhoneNumber != input.PhoneNumber)
            {
                var result1 = await _baseCustomer.CheckIfPhoneNumberIsUnique(input.PhoneNumber);
                if (!result1)
                {
                    throw new Exception(LocalizationHelper.GetLocalizedString("PhoneNumber") + " " + 
                        LocalizationHelper.GetLocalizedString("DuplicateValidation"));

                }
            }
            await UpdateUser(input,customer.IdentityUserId);
            return await base.UpdateAsync(id, input);
        }


    }

    private async Task UpdateUser(CreateUpdateCustomerDto input,Guid userId)
    {
        var user = ObjectMapper.Map<CreateUpdateCustomerDto, UpdateUserDto>(input);
        await _usersAppService.UpdateUserWithoutRoleChangeAsync(userId,user);
    }

    /// <inheritdoc/>

    public override async Task DeleteAsync(Guid id)
    {
        var customer = await _repository.FindAsync(x => x.Id == id);
        if (customer == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Customer") +" "+
                LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        await base.DeleteAsync(id);

    }


}
