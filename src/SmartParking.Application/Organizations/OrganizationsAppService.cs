using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartParking.Authentication;
using SmartParking.CustomerCategories;
using SmartParking.Entities;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Lookups;
using SmartParking.Repositories;
using SmartParking.Results;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectExtending;
using Volo.Abp.TenantManagement;

namespace SmartParking.Organizations;

[Authorize]
public class OrganizationsAppService : CrudAppService<Organization, OrganizationDto, int, PagedAndSortedResultRequestDto, CreateUpdateOrganiztionDto>, IOrganizationsAppService
{
    private readonly IRepository<Organization, int> _repository;
    private readonly ICustomRoleReository _identityRole;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IUsersAppService _usersAppService;
    private readonly ICustomerCategoryAppService _customerCategoryAppService;
    private readonly ISubscriptionAppService _subscriptionAppService;
    private readonly ICitiesAppService _cityAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IDataFilter _softDeleteFilter;

    public OrganizationsAppService(IRepository<Organization, int> repository, ICustomRoleReository identityRole, ICustomerCategoryAppService customerCategoryAppService, IDataFilter softDeleteFilter, IUsersAppService usersAppService, ISubscriptionAppService subscriptionAppService, ITenantManager tenantManager, IRepository<Tenant, Guid> tenantRepository, IRepository<Garage, int> garageRepository, ICitiesAppService cityAppService) : base(repository)
    {
        _repository = repository;
        _identityRole = identityRole;
        _usersAppService = usersAppService;
        _customerCategoryAppService = customerCategoryAppService;
        _softDeleteFilter = softDeleteFilter;
        _subscriptionAppService = subscriptionAppService;
        _tenantManager = tenantManager;
        _tenantRepository = tenantRepository;
        _garageRepository = garageRepository;
        _cityAppService = cityAppService;
    }
    ///<inheritdoc/>
    public override async Task<OrganizationDto> CreateAsync(CreateUpdateOrganiztionDto input)
    {
        Tenant tenant = await CreateTenat(input);
        var role = await AddRole(tenant.Id);
        await CheckCommercial(input);
        Organization organization = await CreateOrganization(input, tenant);
        await AddUser(input, tenant, organization, role);
        organization.Code = CodeGenerator(organization.Id);
        await CurrentUnitOfWork.SaveChangesAsync();
        await _customerCategoryAppService.CreateAsync(new CreateUpdateCutomerCategoryDto { Name = EnumValues.GetEnumDescription(CustomerCategoriesDefaults.Public), Description = EnumValues.GetEnumDescription(CustomerCategoriesDefaults.AllCustomers), Default = true, OrganizationId = organization.Id, CreatedByOrganization = true });
        return ObjectMapper.Map<Organization, OrganizationDto>(organization);
    }

    private async Task<IdentityRole?> AddRole(Guid tenantId)
    {
        var roleUser = new IdentityRole(GuidGenerator.Create(), RolesType.Organization.ToString(), tenantId);
        IEnumerable<IdentityRole> roles = new List<IdentityRole>
        {
            roleUser,
            new IdentityRole(GuidGenerator.Create(),RolesType.Keeper.ToString(), tenantId)
        };
        await _identityRole.InsertManyAsync(roles, true);
        return roleUser;
    }

    private async Task AddUser(CreateUpdateOrganiztionDto input, Tenant tenant, Organization organization,IdentityRole role)
    {
        UserCreateDto user = new UserCreateDto()
        {
            UserName = input.UserName,
            PhoneNumber = input.WhatsAppNum,
            Email = input.Email,
            Password = "P@ssw0rd",
            Name = input.EnglishName,
            IsActive = true,
            UserType = 1,
            OrganizationId = organization.Id,
            TenantId = tenant.Id,
            IdentificationTypes=IdentificationTypes.NationalID,
            NationalId=input.NationalId

        };

        var userCreated = await _usersAppService.CreateWithRoleAsync(user,role.Id);
    }

    private async Task<Organization> CreateOrganization(CreateUpdateOrganiztionDto input, Tenant tenant)
    {
        var city = await _cityAppService.GetAsync(input.CityId);
        var organization = ObjectMapper.Map<CreateUpdateOrganiztionDto, Organization>(input);

        CheckAdress(input, city, organization);
        organization.TenantId = tenant.Id;
        organization.SubscriptionId = input.SubscriptionId;
        await _repository.InsertAsync(organization, autoSave: true);
        return organization;
    }

    private static void CheckAdress(CreateUpdateOrganiztionDto input, CityDto city, Organization organization)
    {
        bool containsArabic = Regex.IsMatch(input.StreetName, @"\p{IsArabic}");
        organization.Address = input.BuildingNumber.ToString() + " ," + input.StreetName + " ," + input.SecondaryNumber.ToString() + " ," + input.District + " ,";
        organization.Address += containsArabic ? city.ArabicName : city.EnglishName;
        organization.Address += " , " + input.PostalCode;
    }

    private async Task CheckCommercial(CreateUpdateOrganiztionDto input)
    {
        var query = await _repository.GetListAsync(x => x.CommercialRegistrationNumber == input.CommercialRegistrationNumber);
        Console.WriteLine($"count of Commercial Registration Number {query.Count}");
        if (query.Count > 0)
        {
            throw new BusinessException(EnumValues.GetEnumDescription(OrganizationCreationValidations.CommercialRegistrationNumber) + query.First().ArabicName);

        }
    }

    private async Task<Tenant> CreateTenat(CreateUpdateOrganiztionDto input)
    {
        var tenant = await _tenantManager.CreateAsync(input.UserName);
        var input1 = new TenantCreateDto
        {
            Name = input.UserName,
            AdminEmailAddress = input.Email,
            AdminPassword = "P@ssw0rd",
        };
        input1.MapExtraPropertiesTo(tenant);
        await CurrentUnitOfWork.SaveChangesAsync();
        await _tenantRepository.InsertAsync(tenant);
        tenant.CreatorId = CurrentUser.Id;
        await CurrentUnitOfWork.SaveChangesAsync();
        return tenant;
    }

    ///<inheritdoc/>
    public override async Task<PagedResultDto<OrganizationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        
            using (_softDeleteFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
    }
    ///<inheritdoc/>
    public override async Task<OrganizationDto> GetAsync(int id)
    {
        using (_softDeleteFilter.Disable<IMultiTenant>())
        {
            var organization = await IsFoundOrganizationAsync(id);
            return ObjectMapper.Map<Organization, OrganizationDto>(organization);

        }
        

    }


    [RemoteService(isEnabled:false)]
    ///<inheritdoc/>
    private string CodeGenerator(int id)
    {
        var idLen =(int) Math.Floor(Math.Log10(id) + 1);
        int zeros = (6 - idLen);
        string Code = "O";
        Code = Code.PadRight(zeros + 1, '0');
        Code += (id);
        return Code;
    }
    ///<inheritdoc/>


    public new async Task<string> DeleteByIdAsync(int id)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var orgaization = await IsFoundOrganizationAsync(id);
            await _repository.HardDeleteAsync(orgaization);

              return EnumValues.GetEnumDescription(OrganizationCreationValidations.OrganizationDeleteion);
            
        }
    }
    ///<inheritdoc/>

    public override async Task<OrganizationDto> UpdateAsync(int id, CreateUpdateOrganiztionDto input)
    {
        using (_softDeleteFilter.Disable<IMultiTenant>())
        {
            var organization = await IsFoundOrganizationAsync(id);
            var city = await _cityAppService.GetAsync(input.CityId);
            if (organization.Address != input.Address || organization.Address==null) {
                CheckAdress(input, city, organization);
                input.Address= organization.Address;
            }


            return await base.UpdateAsync(id, input);
        }
    }

    ///<inheritdoc/>
    public async Task<PagedResultDto<OrganizationDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
            using (_softDeleteFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            queryable = queryable.Include(s => s.Subscription);
            queryable = ApplySearch(input, queryable);
            var sortAndPaginate = await ApplySortAndpaginate(input,queryable);
            var organizations = await AsyncExecuter.ToListAsync(sortAndPaginate.Item2);
            var organizationsDto = ObjectMapper.Map<List<Organization>, List<OrganizationDto>>(organizations);
            return new PagedResultDto<OrganizationDto>(
                sortAndPaginate.Item1,
                organizationsDto
            );
        }
    }

    private static IQueryable<Organization> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Organization> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Subscription.ParkingSlot.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Subscription.NumbersOfGarages.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Code.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Country.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Country.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.State.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.State.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.City.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.City.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Status.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) 
            );
        }

        return queryable;
    }

    private async Task<(int, IQueryable<Organization>)> ApplySortAndpaginate(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Organization> queryable)
    {
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
        return (totalCount,queryable);
    }

    ///<inheritdoc/>
    public async Task<PagedResultDto<OrganizationDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_softDeleteFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            queryable = queryable.Include(x => x.Subscription);
            var filterExpression = BuildExpression<Organization>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            var sortAndPaginate = await ApplySortAndpaginate(input, queryable);
            var organizations = await AsyncExecuter.ToListAsync(sortAndPaginate.Item2);
            var organizationsDto = ObjectMapper.Map<List<Organization>, List<OrganizationDto>>(organizations);
            return new PagedResultDto<OrganizationDto>(
                sortAndPaginate.Item1,
                organizationsDto
            );
        }
    }
    ///<inheritdoc/>
    [RemoteService(IsEnabled = false)]
    public async Task<Result<List<OrganizationDto>>> GetAllMobileAsync()
    {
        using (_softDeleteFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(_ => _.IsDeleted != true && _.Status != Status.Deleted && _.Status != Status.Inactive);
            var organizations = await AsyncExecuter.ToListAsync(query);
            var organizationsMap = ObjectMapper.Map<List<Organization>, List<OrganizationDto>>(organizations);
            if (organizationsMap.Count == 0)
            {
                return Result<List<OrganizationDto>>.NotFound();
            }

            return Result<List<OrganizationDto>>.Success(organizationsMap);

        }

    }

    ///<inheritdoc/>
    public async Task<SubscriptionDto> AddOrganizationForSubscription(int OrganizationId, int SubscriptionId)
    {
        using (_softDeleteFilter.Disable<IMultiTenant>())
        {
            Organization? organization = await IsFoundOrganizationAsync(OrganizationId);
            await IsFoundSubscriptionAsync(SubscriptionId);

            organization.SubscriptionId = SubscriptionId;
            var result = await _repository.UpdateAsync(organization, autoSave: true);
            if (result.Subscription is null)
            {
                throw new BusinessException("Failed Try again");
            }
            return await _subscriptionAppService.GetAsync(SubscriptionId);
        }
    }

    private async Task IsFoundSubscriptionAsync(int SubscriptionId)
    {
        var subscription = await _subscriptionAppService.GetAsync(SubscriptionId);
        if (subscription is null)
        {
            throw new EntityNotFoundException(typeof(Subscription), SubscriptionId);
        }
    }

    private async Task<Organization?> IsFoundOrganizationAsync(int OrganizationId)
    {
        var organization = await _repository.GetAsync(OrganizationId);
        if (organization is null)
        {
            throw new EntityNotFoundException(typeof(Organization), OrganizationId);
        }

        return organization;
    }

    ///<inheritdoc/>
    public async Task<int> GetOrganizationsCount()
    {
        using (_softDeleteFilter.Disable<IMultiTenant>())
        {

            var queryable = await _repository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            return totalCount;
        }
    }
    ///<inheritdoc/>
    public async Task<int> GetOrganizationIdByTenant(Guid? tenantId)
    {
        using(DataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
            query = query.Where(org => org.TenantId == tenantId);
            var org = await AsyncExecuter.FirstOrDefaultAsync(query);
            return org.Id;

        }
    }
    [RemoteService(isEnabled: false)]
    ///<inheritdoc/>
    public async Task<Organization> GetOrganizationForCurrentTenatAsync()
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(org => org.TenantId == CurrentTenant.Id);
        var org = await AsyncExecuter.FirstOrDefaultAsync(query);
        return org;
        }
    }
    ///<inheritdoc/>
    public async Task<int> GetNumberOfGaragesAllowedToBeAdded()
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

        var orgId=await _usersAppService.GetOrganizationIdByTenantAsync(CurrentTenant.Id);
        var query =await _repository.GetQueryableAsync();
        query=query.AsNoTracking();
        query = query.Include(x => x.Subscription);
        var organization =await query.FirstOrDefaultAsync(x=>x.Id==orgId);

        var maxNumberOfGarages=organization?.Subscription?.NumbersOfGarages ?? 0;
        
        return maxNumberOfGarages ;
        }
    }

    public async Task<int> GetNumberOfActiveGaragesOfOrganization()
    {
        var query =await _garageRepository.GetQueryableAsync();
        query = query.Where(x => x.Status == Enums.Status.Active);
        var count = query.Count();
        return count;
        
    }

    [RemoteService(isEnabled: false)]

    ///<inheritdoc/>

    public async Task<string> GetOrganizationPictureByOrganizationTenant(Guid ?OrganizationTenantId)
    {
        var orgId = await GetOrganizationIdByTenant(OrganizationTenantId);
        var organization = await GetAsync(orgId);
        return organization.OrganizationPicture;
    }


}
 