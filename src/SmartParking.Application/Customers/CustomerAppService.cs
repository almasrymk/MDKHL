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

namespace SmartParking.Customers;

public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>, ICustomerAppService
{
    private readonly IRepository<Customer, Guid> _repository;
    private readonly IRepository<Vehicle, int> _repositoryVehicle;
    private IOrganizationsAppService _organizationAppService;
    private  IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;


    public CustomerAppService(IRepository<Customer, Guid> repository, IRepository<Vehicle, int> repositoryVehicle, IDataFilter dataFilter, ICurrentUser currentUser, IOrganizationsAppService organizationAppService, UserManager<Volo.Abp.Identity.IdentityUser> userManager) : base(repository)
    {
        _repository = repository;
        _repositoryVehicle = repositoryVehicle;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _organizationAppService = organizationAppService;
        _userManager = userManager;
    }



    [RemoteService(isEnabled: false)]
    /// <inheritdoc/>
    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(1000, 9999).ToString();
    }


    /// <inheritdoc/>
    public async override Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        try
        {
            var result1= await CheckIfPhoneNumberIsUnique(input.MobileNumber);
            if (result1 == false)
            {
                throw new Exception("Phone Number is used");

            }
            var org = await _organizationAppService.GetOrganizationForCurrentTenat();
            var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
            customer.Organizations = new List<Organization>();
            customer.Organizations.Add(org);
            customer.IdentityUser.Name = input.UserName;
            customer.IdentityUser.Surname = input.FullName;
            customer.IdentityUser.SetProperty("OtpCode", null);
            customer.IdentityUser.SetProperty("OtpExpirationDate", DateTime.Now);
            customer.IdentityUser.SetProperty("UserTypeId", 3);
            customer.IdentityUser.SetProperty("OrganizationId", org.Id);
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
            await _repository.InsertAsync(customer,true);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);
        }
        catch (Exception ex)
        {

            throw new UserFriendlyException("Registration failed: " + ex.Message);
        }
    }
    [RemoteService(isEnabled: false)]
    /// <summary>
    /// Method used to fromat messages that has been thrown in exception
    /// </summary>
    /// <param name="result">IdentityResult type that has message</param>
    /// <returns>String of contains message</returns>
    public string FormatErrorMessage(IdentityResult result)
    {
        string message = string.Empty;
        foreach (var error in result.Errors)
        {
            message += error.Description;
        }
        return message;
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyOtpAsync(OtpVerificationDto input)
    {
        var cutsomer = await _repository.FindAsync(input.CustomerId);
        if (cutsomer == null) {

            throw new UserFriendlyException("No User matched that criteria in Database" );

        }
        if (cutsomer.OTP == input.OtpCode)
        {
            return true;
        }
        return false;

    }
    [RemoteService(isEnabled: false)]
    /// <inheritdoc/>
    public async Task<bool> CheckIfPhoneNumberIsUnique(string phoneNumber) 
    {
        var query = await _repository.GetQueryableAsync();
        query = query.AsNoTracking();
        var result = query.Where(x => x.IdentityUser.PhoneNumber == phoneNumber).ToList();

        if (result.Count == 0) {
            return true;
        
        }
        return false;

    }

	/// <inheritdoc/>
	public async Task<PagedResultDto<CustomerDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
	{
		var queryable = await _repository.GetQueryableAsync();
		if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
		{

			queryable = queryable.Where(or =>
				or.IdentityUser.Name.StartsWith(input.SearchingTerm)

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

	/// <inheritdoc/>
	public async Task<PagedResultDto<CustomerDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
	{
		// Get the IQueryable<customer> from the repository
		var queryable = await _repository.GetQueryableAsync();
		// Apply filtering
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

    /// <inheritdoc/>
    public async Task<List<VehicleDto>> GetVehiclesRelatedToSpecificCustomer(Guid customerId)
    {
        var customer = await GetAsync(customerId);
        if( customer == null)
        {
            throw new Exception("No Customer Found");
        }

        var query = await _repositoryVehicle.GetQueryableAsync();

        query=query.Where(x=>x.CustomerId == customerId);
        var result = await AsyncExecuter.ToListAsync(query);
        var output =ObjectMapper.Map<List<Vehicle>,List<VehicleDto>>(result);
        return output ;
    }


    /// <inheritdoc/>
    public async Task<PagedResultDto<CustomerDto>> GetCustomerByOrganization(PagedAndSortedResultRequestDto input, int organizationId)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.Organizations.Any( org=> org.Id == organizationId));
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

    /// <inheritdoc/>
    public async override Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }
        else
            return await base.GetListAsync(input);
    }


    [RemoteService(isEnabled: false)]
    public async Task<int> GetCountWithoutMultitenant()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        }
    }

    [RemoteService(isEnabled: false)]
    public async Task<int> GetAllCustomersCount()
    {
        if (_currentUser.IsInRole(RolesType.Mdkhl.ToString()) || _currentUser.IsInRole("admin"))
        {

            var countt = await GetCountWithoutMultitenant();
            return countt;
            
        }
        

            var query = await _repository.GetQueryableAsync();
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        
    }
}
