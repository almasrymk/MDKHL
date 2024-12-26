using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

using System.Linq.Dynamic.Core;
using SmartParking.Vehicles;
namespace SmartParking.Customers;

[Authorize]
public class EmployeeAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>, IEmployeeAppService
{
    private readonly IRepository<Customer, Guid> _repository;
    private readonly IRepository<Vehicle, int> _repositoryVehicle;
    private IOrganizationsAppService _organizationAppService;
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
    private readonly IBaseCustomerAppService _baseCustomer;

    public EmployeeAppService(IRepository<Customer, Guid> repository, IRepository<Vehicle, int> repositoryVehicle, IDataFilter dataFilter, ICurrentUser currentUser,
        IOrganizationsAppService organizationAppService, UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        IRepository<CustomerPackage, int> customerPackageRepository,IBaseCustomerAppService baseCustomer) : base(repository)
    {
        _repository = repository;
        _repositoryVehicle = repositoryVehicle;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
        _organizationAppService = organizationAppService;
        _userManager = userManager;
        _baseCustomer = baseCustomer;
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
                    return await _baseCustomer.UserIsFound(user);
                }

            }
            await _baseCustomer.CheckCustomer(input);

            var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
            await AddUser(input, customer);
            customer.IsEmployee = true;
            var customerAdd = await _repository.InsertAsync(customer, true);
            //await _baseCustomer.AddCustomerPackage(organization.Id, customerAdd.Id);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);
        }
        catch (Exception ex)
        {

            throw new UserFriendlyException("Registration failed: " + ex.Message);
        }
    }
    /// <inheritdoc/>
    public async Task<CustomerDto> CreateMdkhlAsync(CreateUpdateCustomerDto input,int organizationId)
    {
        try
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var organization = await _organizationAppService.GetAsync(organizationId);
                if(organization is null)
                {
                    throw new UserFriendlyException("Organization Not Found");
                }
                var user = await _userManager.FindByNameAsync(input.UserName);
                if (user != null)
                {
                    return await _baseCustomer.UserIsFound(user);

                }

            }
            await _baseCustomer.CheckCustomer(input);

            var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
            await AddUser(input, customer);
            customer.IsEmployee = true;
            var customerAdd = await _repository.InsertAsync(customer, true);
            //await _baseCustomer.AddCustomerPackage(organizationId, customerAdd.Id);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);
        }
        catch (Exception ex)
        {

            throw new UserFriendlyException("Registration failed: " + ex.Message);
        }
    }

    /// <inheritdoc/>
    public async override Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var organization = await _organizationAppService.GetOrganizationForCurrentTenat();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
            query = query.Where(x => x.CustomerPackages.Any(org => org.OrganizationId == organization.Id) && x.IsEmployee);
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
            query = query.Where(x => x.IsEmployee==true);
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
    public async Task<PagedResultDto<CustomerDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var queryable = await _repository.GetQueryableAsync();

            queryable = queryable.Include(x => x.IdentityUser);
            var orgid = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenant.Id);

            queryable = queryable.Where(c => c.IsEmployee == true);
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
            queryable = queryable.Where(c => c.IsEmployee == true);
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


            queryable = queryable.Where(c => c.IsEmployee == true);
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
	public async Task<PagedResultDto<CustomerDto>> FilterMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {

            var queryable = await _repository.GetQueryableAsync();


            queryable = queryable.Where(c => c.IsEmployee == true);
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
    /// <summary>
    /// Asynchronously adds a user based on the provided employee data.
    /// </summary>
    /// <param name="input">The DTO containing employee details for user creation.</param>
    /// <param name="customer">The employee entity to associate with the new user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task AddUser(CreateUpdateCustomerDto input, Customer customer)
    {
        customer.IdentityUser.Surname = input.FullName;
        customer.IdentityUser.SetProperty("OtpCode", null);
        customer.IdentityUser.SetProperty("OtpExpirationDate", DateTime.Now);
        customer.IdentityUser.SetProperty("UserTypeId", 3);
        customer.IdentityUser.SetProperty("TenantId", null);
        customer.TenantId = null;
        customer.IdentityUser.Name = input.FullName;
        var createdUser = await _userManager.CreateAsync(customer.IdentityUser, input.Password);
        await _userManager.SetPhoneNumberAsync(customer.IdentityUser, input.PhoneNumber);
        if (!createdUser.Succeeded)
        {
            throw new Exception(message: createdUser.Errors.FirstOrDefault()?.Description);
        }
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var output = await _userManager.AddToRolesAsync(customer.IdentityUser, [RolesType.OrganizationEmployee.ToString()]);
        }
    }


    public async override Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
    {

        bool emailValid = _baseCustomer.IsEmailValid(input.Email);
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
    public override async Task<CustomerDto> GetAsync(Guid id)
    {
        var orgId = await _organizationAppService.GetOrganizationIdByTenant(CurrentTenant.Id);
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _repository.GetAsync(c => c.Id == id && c.IsEmployee);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);

        }
    }


    /// <inheritdoc/>
    public async Task<List<VehicleDto>> GetVehiclesRelatedToSpecificEmployee(Guid employeeId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query =await _repository.GetQueryableAsync();
            query=query.Where(x=>x.Id== employeeId && x.IsEmployee==true);
            var customer =await AsyncExecuter.FirstOrDefaultAsync(query);
            if (customer == null)
            {
                throw new Exception("No Customer Found");
            }

            var queryVehicles = await _repositoryVehicle.GetQueryableAsync();

            queryVehicles = queryVehicles.Where(x => x.CustomerId == employeeId);
            var result = await AsyncExecuter.ToListAsync(queryVehicles);
            var output = ObjectMapper.Map<List<Vehicle>, List<VehicleDto>>(result);
            return output;
        }

    }



}
