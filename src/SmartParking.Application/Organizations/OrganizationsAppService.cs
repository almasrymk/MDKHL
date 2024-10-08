using Scriban.Syntax;
using SmartParking.Authentication;
using SmartParking.CustomerCategories;
using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Validations.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Organizations;

public class OrganizationsAppService : CrudAppService<Organization, OrganizationDto, int, PagedAndSortedResultRequestDto, CreateUpdateOrganiztionDto>, IOrganizationsAppService
{
    private readonly IRepository<Organization, int> _repository;
    private readonly IUsersAppService _usersAppService;
    private readonly ICustomerCategoryAppService _customerCategoryAppService;
    private readonly IIdentityUserAppService _userAppService;

    public IDataFilter _softDeleteFilter { get; }

    public OrganizationsAppService(IRepository<Organization, int> repository ,IUsersAppService usersAppService, ICustomerCategoryAppService customerCategoryAppService, IDataFilter softDeleteFilter, IIdentityUserAppService userAppService) : base(repository)
    {
        _repository = repository;
        _usersAppService = usersAppService;
        _customerCategoryAppService = customerCategoryAppService;
         _softDeleteFilter = softDeleteFilter;
        _userAppService = userAppService;
    }
    ///<inheritdoc/>
    public override async Task<OrganizationDto> CreateAsync(CreateUpdateOrganiztionDto input)
    {
        UserCreateDto user = new UserCreateDto()
        {
            UserName = input.EnglishName,
            PhoneNumber = input.WhatsAppNum,
            Email = input.ContactMail,
            Password = "P@ssw0rd",
            RoleNames = [RolesType.Organization.ToString()],
            Name = input.EnglishName,
            IsActive = true,
            UserType = 1
        };
       var userCreated =  await _usersAppService.CreateAsync(user);
        var query = await _repository.GetListAsync(x => x.UnifiedLicenseNum == input.UnifiedLicenseNum);
        Console.WriteLine($"count of unified license number {query.Count}");
        if (query.Count>0)
        {
            throw new BusinessException(EnumValues.GetEnumDescription(OrganizationCreationValidations.UnifiedLicenseNumUnifiedLicenseNum) + query.First().ArabicName);

        }
        var organization = ObjectMapper.Map<CreateUpdateOrganiztionDto,Organization>(input);
        organization.UserId = userCreated.Id;
        await _repository.InsertAsync(organization,autoSave:true);
        organization.Code = CodeGenerator(organization.Id);
        await _customerCategoryAppService.CreateAsync(new CreateUpdateCutomerCategoryDto { Name =EnumValues.GetEnumDescription(CustomerCategoriesDefaults.Public), Description=EnumValues.GetEnumDescription(CustomerCategoriesDefaults.AllCustomers), Default = true, OrganizationId = organization.Id , CreatedByOrganization=true});
        return ObjectMapper.Map<Organization, OrganizationDto>(organization);


    }
    ///<inheritdoc/>

    public override async Task<PagedResultDto<OrganizationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (CurrentUser.IsInRole("admin")){

            using (_softDeleteFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }
        
            return await base.GetListAsync(input);
        

    }
    ///<inheritdoc/>
    public override async Task<OrganizationDto> GetAsync(int id)
    {
        if (CurrentUser.IsInRole("admin"))
        {

            using (_softDeleteFilter.Disable<IMultiTenant>())
            {
                return await base.GetAsync(id);
            }
        }
        
        return await base.GetAsync(id);
    }
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
        var orgaization = await GetAsync(id);

        if (orgaization is null)
            throw new EntityNotFoundException(typeof(Organization), id);
        else
        {
            await base.DeleteByIdAsync(id);
            orgaization.Status = Enums.Status.Deleted;

            return EnumValues.GetEnumDescription(OrganizationCreationValidations.OrganizationDeleteion);
        }
    }
    ///<inheritdoc/>

    public override async Task<OrganizationDto> UpdateAsync(int id, CreateUpdateOrganiztionDto input)
    {
        var organization= await GetAsync(id);
        if (organization is null)
        {
                throw new EntityNotFoundException(typeof(Organization), id);

        }
        return await base.UpdateAsync(id, input);
    }

    ///<inheritdoc/>
    public async Task<PagedResultDto<OrganizationDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<organization> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.StartsWith(input.SearchingTerm) ||
                or.EnglishName.StartsWith(input.SearchingTerm) ||
                or.Code.StartsWith(input.SearchingTerm) ||
                or.Country.ArabicName.StartsWith(input.SearchingTerm) ||
                or.Country.EnglishName.StartsWith(input.SearchingTerm) ||
                or.State.EnglishName.StartsWith(input.SearchingTerm) ||
                or.State.ArabicName.StartsWith(input.SearchingTerm) ||
                or.City.EnglishName.StartsWith(input.SearchingTerm) ||
                or.City.ArabicName.StartsWith(input.SearchingTerm)
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
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of hotel entities
        var organizations = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var organizationsDto = ObjectMapper.Map<List<Organization>, List<OrganizationDto>>(organizations);
        Console.WriteLine(organizationsDto.Count);
        // Return the paginated result
        return new PagedResultDto<OrganizationDto>(
            totalCount,
            organizationsDto
        );
    }

    ///<inheritdoc/>
    public async Task<PagedResultDto<OrganizationDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<organization> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Organization>.BuildFilterExpression(input.filters);
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
        // Execute the query and get the list of organizations entities
        var organizations = await AsyncExecuter.ToListAsync(queryable);

        // Map the organizations entities to organizationsDto objects
        var organizationsDto = ObjectMapper.Map<List<Organization>, List<OrganizationDto>>(organizations);
        Console.WriteLine(organizationsDto.Count);
        // Return the paginated result
        return new PagedResultDto<OrganizationDto>(
            totalCount,
            organizationsDto
        );
    }
    ///<inheritdoc/>
    [RemoteService(IsEnabled = false)]
    public async Task<List<OrganizationDto>> GetAllMobileAsync()
    {
        
            var organizations = await _repository.GetListAsync();
            var organizationsMap = ObjectMapper.Map<List<Organization>, List<OrganizationDto>>(organizations);
            return organizationsMap;
        
    }
}
 