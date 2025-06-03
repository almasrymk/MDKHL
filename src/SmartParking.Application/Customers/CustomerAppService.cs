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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using SmartParking.Filters;
namespace SmartParking.Customers;
/// <summary>
/// Customer application service class.
/// </summary>
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
        IBaseCustomerAppService baseCustomer, ICustomerPackagesAppService packagesAppService,
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


    #region public methods
    /// <inheritdoc/>
    public async Task<CustomerDto> CreateMdkhlAsync(CreateUpdateCustomerDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userManager.FindByNameAsync(input.UserName);
            if (user != null)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("User") + " " +
                    LocalizationHelper.GetLocalizedString("DuplicateValidation"));

            }
            await _baseCustomer.CheckCustomer(input);
            var result1 = await _baseCustomer.CheckIfPhoneNumberIsUnique(input.PhoneNumber);
            if (!result1)
            {
                throw new UserFriendlyException(LocalizationHelper.GetLocalizedString("PhoneNumber") + " " +
                    LocalizationHelper.GetLocalizedString("DuplicateValidation"));

            }
            var result2 = await _baseCustomer.CheckIfNationalIDIsUnique(input.NationalId);
            if (!result2)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("NationId") + " " +
                    LocalizationHelper.GetLocalizedString("DuplicateValidation"));

            }
            var customer = ObjectMapper.Map<CreateUpdateCustomerDto, Customer>(input);
            customer.IdentificationTypes = input.IdentificationType;
            UserDto userCreated;
            if (input.IsUser)
            {
                
                userCreated = await AddUser(input);
            }
            else
            {
                
                userCreated = await AddUserForCustomer(input);
            }
            customer.IdentityUserId = userCreated.Id;
            var customerAdd = await _repository.InsertAsync(customer, true);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);

        }


    }
    /// <inheritdoc/>
    [RemoteService(false)]
    public async Task<PagedResultDto<CustomerDto>> GetListAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var organization = await _organizationAppService.GetOrganizationForCurrentTenatAsync();
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
            query = query.Include(c => c.IdentityUser);
            query = query.Where(x => x.CustomerPackages.Any(org => org.OrganizationId == organization.Id));
            query = CreateSearchQuery(input, query);
            query = AddSortQuery(input, query);
            var paged = await AddPaged(input, query);
            query = paged.query;
            List<CustomerDto> cutomersDtos = await AddMap(query, null);
            return new PagedResultDto<CustomerDto>
            {
                TotalCount = paged.totalCount,
                Items = cutomersDtos
            };
        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<CustomerDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Include(c => c.IdentityUser);
            query = query.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
            query = CheckTenant(tenantId, query);
            query = CreateSearchQuery(input, query);
            query = AddSortQuery(input, query);
            var paged = await AddPaged(input, query);
            query = paged.query;
            List<CustomerDto> cutomersDtos = await AddMap(query, null);

            return new PagedResultDto<CustomerDto>
            {
                TotalCount = paged.totalCount,
                Items = cutomersDtos
            };
        }
    }
    /// <inheritdoc/>
    [RemoteService(false)]
    public async Task<PagedResultDto<CustomerDto>> GetListNoPackageAsync(PagedAndSortedAndSearchResultRequestDto input, int? organizationId, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
            query = query.Where(x => !x.CustomerPackages.Any());
            query = CheckTenant(tenantId, query);
            query = IsOrganization(organizationId, query);
            query = CreateSearchQuery(input, query);
            query = AddSortQuery(input, query);
            var paged = await AddPaged(input, query);
            query = paged.query;
            List<CustomerDto> cutomersDtos = await AddMap(query, null);
            return new PagedResultDto<CustomerDto>
            {
                TotalCount = paged.totalCount,
                Items = cutomersDtos
            };
        }
    }
    /// <inheritdoc/>
    [RemoteService(false)]
    public async Task<PagedResultDto<CustomerDto>> GetListForCustomerCategoryAsync(PagedAndSortedAndSearchResultRequestDto input, CustomerCategoryStatus customerCategory, int? organizationId, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();

            query = query.Include(c => c.CustomerPackages).ThenInclude(p => p.Organization);
            query = query.Where(c => c.CustomerPackages.Any(_ => _.CustomerCategory == customerCategory));
            query = CheckTenant(tenantId, query);
            query = IsOrganization(organizationId, query);
            query = CreateSearchQuery(input, query);
            query = AddSortQuery(input, query);
            var paged = await AddPaged(input, query);
            query = paged.query;
            List<CustomerDto> cutomersDtos = await AddMap(query, customerCategory);

            return new PagedResultDto<CustomerDto>
            {
                TotalCount = paged.totalCount,
                Items = cutomersDtos
            };

        }

    }
    /// <inheritdoc/>
    public override async Task<CustomerDto> GetAsync(Guid id)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _repository.GetAsync(c => c.Id == id);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);

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

            queryable = CreateSearchQuery(input, queryable);
            queryable = AddSortQuery(input, queryable);
            var paged = await AddPaged(input, queryable);
            queryable = paged.query;
            List<CustomerDto> cutomersDtos = await AddMap(queryable, null);
            return new PagedResultDto<CustomerDto>(
                paged.totalCount,
                cutomersDtos
            );
        }

    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<CustomerDto>> SearchMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var queryable = await _repository.GetQueryableAsync();
            queryable = queryable.Include(x => x.IdentityUser);
            queryable = CreateSearchQuery(input, queryable);
            queryable = AddSortQuery(input, queryable);
            var paged = await AddPaged(input, queryable);
            queryable = paged.query;
            List<CustomerDto> cutomersDtos = await AddMap(queryable, null);
            return new PagedResultDto<CustomerDto>(
                paged.totalCount,
                cutomersDtos
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
    }
    /// <inheritdoc/>
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
    public async Task<PagedResultDto<VehicleDto>> GetVehiclesRelatedToSpecificCustomerMdkhl(Guid customerId, PagedAndSortedResultRequestDto input)
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
            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of garages entities
            var result = await AsyncExecuter.ToListAsync(query);
            var output = ObjectMapper.Map<List<Vehicle>, List<VehicleDto>>(result);
            return new PagedResultDto<VehicleDto>
            {
                Items = output,
                TotalCount = totalCount
            };
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
            query = query.Where(c => c.IsEmployee == false);
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
    /// <inheritdoc/>
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
    /// <inheritdoc/>
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
            await UpdateUser(input, customer.IdentityUserId);
            return await base.UpdateAsync(id, input);
        }


    }
    /// <inheritdoc/>
    public async Task<int> GetNumberOfCustomerAtOrganizationAsync(int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Include(x => x.CustomerPackages)
                         .Where(x => x.CustomerPackages != null && x.CustomerPackages.Any(o => o.OrganizationId == organizationId && o.ExpiryDate >= DateTime.Now
                         ))
                         .DistinctBy(x => x.Id);
            var count = await AsyncExecuter.CountAsync(query);
            return count;
        }
    }
    #endregion

    #region private methods 
    /// <summary>
    /// Asynchronously adds a user for a customer with multi-tenancy disabled.
    /// </summary>
    /// <param name="input">The DTO containing customer details for user creation.</param>
    /// <returns>The created <see cref="UserDto"/> associated with the customer.</returns>
    private async Task<UserDto> AddUserForCustomer(CreateUpdateCustomerDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var userMap = ObjectMapper.Map<CreateUpdateCustomerDto, UserCreateDto>(input);
            userMap.TenantId = null;
            userMap.OrganizationId = 0;
            var role = await _authenticationUtility.GetRoleByNameAndTenant(RolesType.Customer.ToString(), null);
            var user = await _usersAppService.CreateWithRoleAsync(userMap, role.Id);
            return user;
        }
    }

    /// <summary>
    /// Asynchronously adds a user without assigning it to a customer directly.
    /// </summary>
    /// <param name="input">The DTO containing customer details and role ID for user creation.</param>
    /// <returns>The created <see cref="UserDto"/>.</returns>
    private async Task<UserDto> AddUser(CreateUpdateCustomerDto input)
    {

        var userMap = ObjectMapper.Map<CreateUpdateCustomerDto, UserCreateDto>(input);
        userMap.TenantId = input.TenantId;
        userMap.IsCustomer = true;
        var user = await _usersAppService.CreateWithRoleWithoutCustomerAsync(userMap, input.RoleId);
        return user;
    }
    /// <summary>
    /// Filters customers based on the tenant ID.
    /// </summary>
    /// <param name="tenantId">The optional tenant ID.</param>
    /// <param name="query">The customer queryable to apply filtering to.</param>
    /// <returns>Filtered <see cref="IQueryable{Customer}"/>.</returns>
    private static IQueryable<Customer> CheckTenant(Guid? tenantId, IQueryable<Customer> query)
    {
        if (tenantId.HasValue)
            query = query.Where(x => x.CustomerPackages.Any(org => org.TenantId == tenantId.Value));
        return query;
    }
    /// <summary>
    /// Filters customers based on the organization ID.
    /// </summary>
    /// <param name="organizationId">The optional organization ID.</param>
    /// <param name="query">The customer queryable to apply filtering to.</param>
    /// <returns>Filtered <see cref="IQueryable{Customer}"/>.</returns>
    private static IQueryable<Customer> IsOrganization(int? organizationId, IQueryable<Customer> query)
    {
        if (organizationId is not null)
        {
            query = query.Where(x => x.CustomerPackages.Any(org => org.OrganizationId == organizationId));
        }

        return query;
    }
    /// <summary>
    /// Maps the filtered customer query to a list of <see cref="CustomerDto"/>, applying optional category and tenant filters.
    /// </summary>
    /// <param name="query">The filtered customer queryable.</param>
    /// <param name="customerCategory">Optional customer category to filter by.</param>
    /// <returns>List of <see cref="CustomerDto"/> mapped from the query.</returns>
    private async Task<List<CustomerDto>> AddMap(IQueryable<Customer> query, CustomerCategoryStatus? customerCategory)
    {

        List<CustomerDto> customersDtos = await query
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                ArabicName = c.ArabicName,
                FullName = c.IdentityUser.Name,
                Email = c.IdentityUser.Email,
                MobileNumber = c.IdentityUser.PhoneNumber,
                NationalId = c.NationalId,
                _packageEnglishNames = string.Join(", ", c.CustomerPackages
                           .Where(_ =>
                               (!customerCategory.HasValue || _.CustomerCategory == customerCategory) && (CurrentTenant.IsAvailable? _.TenantId == CurrentTenant.Id.Value : true))
                           .Select(cp => cp.PackageEnglishName)
                           .Where(name => !string.IsNullOrWhiteSpace(name))),
                _packageArabicNames = string.Join(", ", c.CustomerPackages
                           .Where(_ =>
                               (!customerCategory.HasValue || _.CustomerCategory == customerCategory) && (CurrentTenant.IsAvailable ? _.TenantId == CurrentTenant.Id.Value : true))
                           .Select(cp => cp.PackageArabicName)
                           .Where(name => !string.IsNullOrWhiteSpace(name))),
                OrganizationArabicName = string.Join(", ", c.CustomerPackages.Where(_ => _.PackageId != null)
                                                    .Select(cp => cp.Organization.ArabicName).Distinct()),
                OrganizationEnglishName = string.Join(", ", c.CustomerPackages.Where(_ => _.PackageId != null)
                                                    .Select(cp => cp.Organization.EnglishName).Distinct()),
            })
            .ToListAsync();

        return customersDtos;
    }

    /// <summary>
    /// Adds pagination to the customer query based on the input and returns the result.
    /// </summary>
    /// <param name="input">Pagination and sorting details.</param>
    /// <param name="query">The query to paginate.</param>
    /// <returns>A tuple containing the total count and the paginated query.</returns>
    private async Task<(int totalCount, IQueryable<Customer> query)> AddPaged(PagedAndSortedResultRequestDto input, IQueryable<Customer> query)
    {
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        return (totalCount, query);
    }
    /// <summary>
    /// Applies sorting to the customer query based on the input parameters.
    /// </summary>
    /// <param name="input">Sorting input parameters.</param>
    /// <param name="query">The customer query to sort.</param>
    /// <returns>Sorted <see cref="IQueryable{Customer}"/>.</returns>
    private static IQueryable<Customer> AddSortQuery(PagedAndSortedResultRequestDto input, IQueryable<Customer> query)
    {
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

        return query;
    }

    /// <summary>
    /// Applies search filters to the customer query based on the search term in the input.
    /// </summary>
    /// <param name="input">Search input parameters.</param>
    /// <param name="queryable">The queryable to apply search filters on.</param>
    /// <returns>Filtered <see cref="IQueryable{Customer}"/> with search conditions.</returns>
    private static IQueryable<Customer> CreateSearchQuery(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Customer> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.IdentityUser.UserName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.IdentityUser.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.NationalId.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.IdentityUser.Email.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.IdentityUser.PhoneNumber.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.IdentityUser.Surname.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower())
            );

        }

        return queryable;
    }
    /// <summary>
    /// Updates the user associated with a customer, excluding role changes.
    /// </summary>
    /// <param name="input">Updated customer information.</param>
    /// <param name="userId">The ID of the user to update.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    private async Task UpdateUser(CreateUpdateCustomerDto input, Guid userId)
    {
        var user = ObjectMapper.Map<CreateUpdateCustomerDto, UpdateUserDto>(input);
        await _usersAppService.UpdateUserWithoutRoleChangeAsync(userId, user);
    }
    #endregion
    #region UnUsed Methods
    public override async Task DeleteAsync(Guid id)
    {
        var customer = await _repository.FindAsync(x => x.Id == id);
        if (customer == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Customer") + " " +
                LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        await _repository.HardDeleteAsync(customer);

    }
    #endregion

}
