using Scriban.Syntax;
using SmartParking.CustomerCategories;
using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Validations.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Organizations;

public class OrganizationsAppService : CrudAppService<Organization, OrganizationDto, int, PagedAndSortedResultRequestDto, CreateUpdateOrganiztionDto>, IOrganizationsAppService
{
    private readonly IRepository<Organization, int> _repository;
    private readonly ICustomerCategoryAppService _customerCategoryAppService;

    public IDataFilter _softDeleteFilter { get; }

    public OrganizationsAppService(IRepository<Organization, int> repository , ICustomerCategoryAppService customerCategoryAppService, IDataFilter softDeleteFilter) : base(repository)
    {
        _repository = repository;
        _customerCategoryAppService = customerCategoryAppService;
         _softDeleteFilter = softDeleteFilter;
    }
    /// <summary>
    /// methods that creates an organization
    /// checks if an organization with same UnifiedLicenseNum
    /// if it exists it throws an exception
    /// </summary>
    /// <param name="input">Organization Create Dto</param>
    /// <returns><OrganizationDto</returns>
    /// <exception cref="BusinessException">throws an exception when same UnifiedLicenseNum exists </exception>
    public override async Task<OrganizationDto> CreateAsync(CreateUpdateOrganiztionDto input)
    {
        var query = await _repository.GetListAsync(x => x.UnifiedLicenseNum == input.UnifiedLicenseNum);
        Console.WriteLine($"count of unified license number {query.Count}");
        if (query.Count>0)
        {
            throw new BusinessException(EnumValues.GetEnumDescription(OrganizationCreationValidations.UnifiedLicenseNumUnifiedLicenseNum) + query.First().ArabicName);

        }
        var organization = ObjectMapper.Map<CreateUpdateOrganiztionDto,Organization>(input);
        await _repository.InsertAsync(organization,autoSave:true);
        organization.Code = CodeGenerator(organization.Id);
        await _customerCategoryAppService.CreateAsync(new CreateUpdateCutomerCategoryDto { Name =EnumValues.GetEnumDescription(CustomerCategoriesDefaults.Public), Description=EnumValues.GetEnumDescription(CustomerCategoriesDefaults.AllCustomers), Default = true, OrganizationId = organization.Id , CreatedByOrganization=true});
        return ObjectMapper.Map<Organization, OrganizationDto>(organization);


    }
    /// <summary>
    /// method used to get list of organizations
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not</param>
    /// <returns>OrganizatioDto List</returns>

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
    /// <summary>
    /// method to get organization by its id
    /// </summary>
    /// <param name="id">organizationID</param>
    /// <returns>OrganizationDto</returns>
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
    /// <summary>
    /// method used to create unique code of organization depends on its id
    ///
    /// </summary>
    /// <param name="id">OrganizationId</param>
    /// <returns>StringContainsOrganizationCode</returns>
    private string CodeGenerator(int id)
    {
        var idLen =(int) Math.Floor(Math.Log10(id) + 1);
        int zeros = (6 - idLen);
        string Code = "O";
        Code = Code.PadRight(zeros + 1, '0');
        Code += (id);
        return Code;
    }
    /// <summary>
    /// method to soft delete organization 
    /// checks if organization exists
    /// </summary>
    /// <param name="id">organizationID</param>
    /// <returns>Successfully Deletion message</returns>
    /// <exception cref="EntityNotFoundException">Exception is thrown when Organization Not found</exception>


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
    /// <summary>
    /// method to Update organization 
    /// checks if organization exists
    /// </summary>
    /// <param name="id">organizationID</param>
    /// <returns>Organization Dto of updated version</returns>
    /// <exception cref="EntityNotFoundException">Exception is thrown when Organization Not found</exception>

    public override async Task<OrganizationDto> UpdateAsync(int id, CreateUpdateOrganiztionDto input)
    {
        var organization= await GetAsync(id);
        if (organization is null)
        {
                throw new EntityNotFoundException(typeof(Organization), id);

        }
        return await base.UpdateAsync(id, input);
    }

    /// <summary>
    /// method used to get list of organizations
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not</param>
    /// <returns>OrganizatioDto List</returns>
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

    /// <summary>
    /// method used to get list of organizations
    /// </summary>
    /// <param name="input">specifies max number of organization and if it filtered or not</param>
    /// <returns>OrganizatioDto List</returns>
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
}
 