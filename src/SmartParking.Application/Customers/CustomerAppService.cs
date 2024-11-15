using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using SmartParking.Entities.Customers;
using Volo.Abp.Application.Dtos;
using SmartParking.Cutomers;
using System.Linq;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using Volo.Abp.Data;
using Abp.ObjectComparators.BooleanComparators;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using SmartParking.Entities.Vehicles;
using SmartParking.Vehicles;
using SmartParking.Entities.Organizations;
using SmartParking.Garages;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Nito.AsyncEx;
using SmartParking.Enums;
using SmartParking.Organizations;
using Microsoft.AspNetCore.Authorization;
using Abp.Domain.Entities;
using static Volo.Abp.UI.Navigation.DefaultMenuNames.Application;
using System.Text.RegularExpressions;
using Castle.Core.Resource;

namespace SmartParking.Customers;
[Authorize]
public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>, ICustomerAppService
{
    private readonly IRepository<Customer, Guid> _repository;
    private readonly IRepository<Vehicle, int> _repositoryVehicle;
    private IOrganizationsAppService _organizationAppService;
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;

    public CustomerAppService(IRepository<Customer, Guid> repository, IRepository<Vehicle, int> repositoryVehicle, IDataFilter dataFilter, ICurrentUser currentUser,
        IOrganizationsAppService organizationAppService, UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        IRepository<CustomerPackage, int> customerPackageRepository) : base(repository)
    {
        _repository = repository;
        _repositoryVehicle = repositoryVehicle;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _organizationAppService = organizationAppService;
        _userManager = userManager;
        _customerPackageRepository = customerPackageRepository;
    }
    /// <inheritdoc/>
    public async Task<string> GenerateOtp(Guid CustomerId)
    {

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.Id == CustomerId);
        var customer = query.FirstOrDefault();

        if (customer == default(Customer))
        {
            throw new EntityNotFoundException(typeof(Customer), CustomerId);

        }
        var random = new Random();
        string otp = random.Next(1000, 9999).ToString();
        customer.OTP = otp;
        customer.OtpExpirationDate = DateTime.UtcNow.AddMinutes(1);
        await _repository.UpdateAsync(customer);
        return otp;

    }


    /// <inheritdoc/>
    public async override Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        try
		{
			var organization = await _organizationAppService.GetOrganizationForCurrentTenat();

			using (_dataFilter.Disable<IMultiTenant>())
			{
				var user = await _userManager.FindByNameAsync(input.UserName);
				if (user != null)
				{
					return await UserIsFound(user);
				}

			}
			await CheckCustomer(input);

			var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
			await AddUser(input, customer);
			var customerAdd = await _repository.InsertAsync(customer, true);
			await AddCustomerPackage(organization, customerAdd);
			return ObjectMapper.Map<Customer, CustomerDto>(customer);
		}
		catch (Exception ex)
        {

            throw new UserFriendlyException("Registration failed: " + ex.Message);
        }
    }

	

	/// <inheritdoc/>
	public async Task<CustomerDto> CreateMdkhlAsync(CreateUpdateCustomerDto input)
	{
		try
		{
			

			using (_dataFilter.Disable<IMultiTenant>())
			{
				var user = await _userManager.FindByNameAsync(input.UserName);
				if (user != null)
				{
					return await UserIsFound(user);

				}

			}
			await CheckCustomer(input);

			var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
			await AddUser(input, customer);
			var customerAdd = await _repository.InsertAsync(customer, true);
			return ObjectMapper.Map<Customer, CustomerDto>(customer);
		}
		catch (Exception ex)
		{

			throw new UserFriendlyException("Registration failed: " + ex.Message);
		}
	}


	/// <summary>
	/// Asynchronously checks if the specified user exists and returns the associated customer data if found.
	/// </summary>
	/// <param name="user">The <see cref="Volo.Abp.Identity.IdentityUser"/> object representing the user to check.</param>
	/// <returns>
	/// A <see cref="CustomerDto"/> object containing customer details if the user is found;
	/// otherwise, <c>null</c>.
	/// </returns>
	/// <remarks>
	/// This method queries the customer information based on the provided user.
	/// If the user does not exist, it returns <c>null</c>.
	/// </remarks>
	private async Task<CustomerDto> UserIsFound(Volo.Abp.Identity.IdentityUser? user)
	{
		var query = await _repository.GetQueryableAsync();
		query = query.Where(x => x.IdentityUserId == user.Id);
		var existCustomer = query.FirstOrDefault();
		await _repository.UpdateAsync(existCustomer);
		return ObjectMapper.Map<Customer, CustomerDto>(existCustomer);
	}

	/// <summary>
	/// Asynchronously checks the validity of a customer based on the provided input data.
	/// </summary>
	/// <param name="input">The data transfer object containing customer details to be validated.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	private async Task CheckCustomer(CreateUpdateCustomerDto input)
	{
		if (input.IdentificationType == IdentificationTypes.NationalID)
		{
			var nationalIdPattern = new Regex(@"^(1|2)\d{9}$");
			if (!nationalIdPattern.IsMatch(input.NationalId))
			{
				throw new UserFriendlyException("Nation Id must be 10 digits");
			}
		}
		else if (input.IdentificationType == IdentificationTypes.Passport)
		{
			var passportPattern = new Regex(@"^(?=.*[a-zA-Z]{0,1})[a-zA-Z0-9]{6,9}$");
			if (!passportPattern.IsMatch(input.NationalId))
			{
				throw new UserFriendlyException("Passport must be from 6 - 9 digits and atmost one character");
			}
		}
		var result1 = await CheckIfPhoneNumberIsUnique(input.MobileNumber);
		if (!result1)
		{
			throw new Exception("Phone Number is used");

		}
		var result2 = await CheckIfNationalIDIsUnique(input.NationalId);
		if (!result2)
		{
			throw new Exception("National Id is already exist !");

		}
	}

	/// <summary>
	/// Asynchronously adds a customer package for a specified organization.
	/// </summary>
	/// <param name="organization">The organization to which the customer package will be added.</param>
	/// <param name="customerAdd">The customer entity containing details for the package to be added.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task AddCustomerPackage(Organization organization, Customer customerAdd)
	{
		var customerPackage = new CustomerPackage
		{
			CustomerId = customerAdd.Id,
			CustomerPackageStatus = CustomerPackageStatus.Active,
			OrganizationId = organization.Id,
			TenantId = null
		};
		await _customerPackageRepository.InsertAsync(customerPackage, true);
	}

	/// <summary>
	/// Asynchronously adds a user based on the provided customer data.
	/// </summary>
	/// <param name="input">The DTO containing customer details for user creation.</param>
	/// <param name="customer">The customer entity to associate with the new user.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	private async Task AddUser(CreateUpdateCustomerDto input, Customer customer)
	{
		customer.IdentityUser.Name = input.UserName;
		customer.IdentityUser.Surname = input.FullName;
		customer.IdentityUser.SetProperty("OtpCode", null);
		customer.IdentityUser.SetProperty("OtpExpirationDate", DateTime.Now);
		customer.IdentityUser.SetProperty("UserTypeId", 3);
		customer.IdentityUser.SetProperty("TenantId", null);
		customer.TenantId = null;
		customer.IdentityUser.Name = input.FullName;
		var createdUser = await _userManager.CreateAsync(customer.IdentityUser, input.Password);
		await _userManager.SetPhoneNumberAsync(customer.IdentityUser, input.MobileNumber);
		if (!createdUser.Succeeded)
		{
			throw new Exception(message: createdUser.Errors.FirstOrDefault()?.Description);
		}
		using (_dataFilter.Disable<IMultiTenant>())
		{
			var output = await _userManager.AddToRolesAsync(customer.IdentityUser, [RolesType.Mdkhl.ToString()]);
		}
	}

    /// <inheritdoc/>
    public async Task<bool> VerifyOtpAsync(OtpVerificationDto input)
    {

        var customer = await _repository.FindAsync(input.CustomerId);

        if (customer == null)
        {

            throw new BusinessException("No User matched that criteria in Database");

        }
        if (customer.OTP == input.OtpCode)
        {
            if (DateTime.UtcNow <= customer.OtpExpirationDate)
            {
                return true;
            }
            else
            {
                throw new BusinessException("your Otp is expired");

            }
        }
        return false;

    }

	/// <inheritdoc/>
    [RemoteService(isEnabled: false)]
	public async Task<bool> CheckIfPhoneNumberIsUnique(string phoneNumber)
    {
        var query = await _repository.GetQueryableAsync();
        var result = query.Where(x => x.IdentityUser.PhoneNumber == phoneNumber).ToList();

        if (result.Count == 0)
        {
            return true;

        }
        return false;

    }


	/// <inheritdoc/>
	public async override Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedResultRequestDto input)
	{
		var organization = await _organizationAppService.GetOrganizationForCurrentTenat();
		using (_dataFilter.Disable<IMultiTenant>())
		{
			var query = await _repository.GetQueryableAsync();
			query = query.Where(x => x.CustomerPackages.Any(org => org.Id == organization.Id));
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
	public async Task<PagedResultDto<CustomerDto>> GetListMdkhlAsync(PagedAndSortedResultRequestDto input)
	{
		using (_dataFilter.Disable<IMultiTenant>())
		{

			var query = await _repository.GetQueryableAsync();
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
        var orgId = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenant.Id);
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);

        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<CustomerDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var queryable = await _repository.GetQueryableAsync();
            
            queryable = queryable.Include(x => x.IdentityUser);
            var orgid = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenant.Id);

            
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                    or.IdentityUser.UserName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.IdentityUser.Surname.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.IdentityUser.Email.ToLower().Contains(input.SearchingTerm.ToLower()) ||
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
                    or.IdentityUser.UserName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.IdentityUser.Surname.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.IdentityUser.Email.ToLower().Contains(input.SearchingTerm.ToLower()) ||
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

            var queryable = await _repository.GetQueryableAsync();
            var orgid = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenant.Id);

            

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
            var orgid = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenant.Id);

            

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
    public async Task<List<VehicleDto>> GetVehiclesRelatedToSpecificCustomer(Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await GetAsync(customerId);
            if (customer == null)
            {
                throw new Exception("No Customer Found");
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
            throw new EntityNotFoundException(typeof(Organization), organizationId);

        }

        var query = await _repository.GetQueryableAsync();
        //query = query.Where(x => x.Organizations.Any(org => org.Id == organizationId));
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

   

	public async Task<int> GetCountCustomersCountForMDKHL()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
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
            //query = query.Include(x => x.Organizations).Where(x => x.Organizations.Any(o => o.Id == orgId));
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        }
    }

    /// <inheritdoc/>

    public async Task<bool> CheckIfNationalIDIsUnique(string NationalId)
    {
        var query = await _repository.GetQueryableAsync();
        var result = query.Where(x => x.NationalId == NationalId).ToList();
        if (result.Count == 0)
        {
            return true;

        }
        return false;

    }

    /// <inheritdoc/>

    public async override Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
    {

        bool emailValid = IsEmailValid(input.Email);
        if (!emailValid)
        {
            throw new Exception("Email is not valid");

        }
        if (input.Password != input.RepeatPassword)
        {
            throw new Exception("Passwords doesn't match");

        }
        var customer = await GetAsync(id);

        if (customer == null)
        {
            throw new Exception("No Customer Found");

        }
        if (customer.NationalId != input.NationalId || customer.identificationTypes != input.IdentificationType)
        {
            throw new Exception("Critical Data as National Id , Identification. can't be updated");
        }

        return await base.UpdateAsync(id, input);

    }

    [RemoteService(isEnabled: false)]
    public bool IsEmailValid(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        var emailvalidation = Regex.IsMatch(email, pattern);
        if (!emailvalidation)
        {
            return false;

        }
        return true;
    }

    /// <inheritdoc/>

    public override async Task DeleteAsync(Guid id)
    {
        var customer = await _repository.FindAsync(x => x.Id == id);
        if (customer == null)
        {
            throw new Exception("There is no customer related to that id");
        }
        await base.DeleteAsync(id);

    }


}
