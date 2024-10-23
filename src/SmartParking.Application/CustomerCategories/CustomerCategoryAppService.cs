using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.CustomerCategories;

[Authorize]
public class CustomerCategoryAppService : CrudAppService<CustomerCategory, CustomerCategoryDto, int, PagedAndSortedResultRequestDto, CreateUpdateCutomerCategoryDto>, ICustomerCategoryAppService
{
    private readonly IRepository<CustomerCategory, int> _repository;
    private List<Organization> organizations;

    public CustomerCategoryAppService(IRepository<CustomerCategory, int> repository) : base(repository)
    {
        _repository = repository;
    }

    public override async Task<CustomerCategoryDto> CreateAsync(CreateUpdateCutomerCategoryDto input)
    {
        if (input.CreatedByOrganization)
        {
            return await base.CreateAsync(input);
        }
        var query = await  _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
        if(query.Count == 0)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        else if(input.Default == true)
        {
            if(query.Count > 0)
            {
                var customerCategory = query.First(x => x.Default == true);
                customerCategory.Default = false;

            }

        }
        return await base.CreateAsync(input);
    }
    public override async Task<CustomerCategoryDto> UpdateAsync(int id, CreateUpdateCutomerCategoryDto input)
    {
        //object has been set to be Default is true (true in database) , and in update operation is set Default to be false
        var customerCategoryById = await _repository.FindAsync(x => x.Id == id);
        if(input.Default==false && customerCategoryById.Default == true)
        {
            throw new BusinessException(EnumValues.GetEnumDescription(CustomerCategoryValidations.DefaultIsTrueAndUpdatedToBeFalse) + " " + customerCategoryById.Organization.EnglishName);
        }


        if (input.Default==true) {
            var query = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
            var customerCategory = query.FirstOrDefault(x => x.Default == true);
            customerCategory.Default = false;
        }
        
        return await base.UpdateAsync(id, input);
    }

    public async Task<PagedResultDto<CustomerCategoryDto>> GetCustomerCategoriesByOrganizationId(int OrganizationId)
    {

        var query =await _repository.GetQueryableAsync();
        query =query.Where(c => c.OrganizationId == OrganizationId);
        var  category = await AsyncExecuter.ToListAsync(query);
        var list= ObjectMapper.Map<List<CustomerCategory>, List<CustomerCategoryDto>>(category);
        return new PagedResultDto<CustomerCategoryDto>(list.Count,list);

    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var customerCategory=await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if (customerCategory.Default == true) {

            throw new BusinessException(EnumValues.GetEnumDescription(CustomerCategoryValidations.DeleteDefaultForOrganization) +" " +customerCategory.Organization.EnglishName );

        }

        await base.DeleteAsync(id);
    }

    /// <summary>
    /// method used to get list of customer category
    /// </summary>
    /// <param name="input">specifies max number of customer category and if it sorted or not</param>
    /// <returns>customer categoryDto List</returns>
    public async Task<PagedResultDto<CustomerCategoryDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<customer category> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.Name.StartsWith(input.SearchingTerm) ||
                or.Description.StartsWith(input.SearchingTerm) ||
                or.Organization.EnglishName.StartsWith(input.SearchingTerm) 
            );
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of customer category entities
        var customerCategories = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to customerCategoriesDto objects
        var customerCategoriesDto = ObjectMapper.Map<List<CustomerCategory>, List<CustomerCategoryDto>>(customerCategories);
        // Return the paginated result
        return new PagedResultDto<CustomerCategoryDto>(
            totalCount,
            customerCategoriesDto
        );
    }

    /// <summary>
    /// method used to get list of customerCategories
    /// </summary>
    /// <param name="input">specifies max number of customerCategories and if it filtered or not</param>
    /// <returns>customerCategoriesDto List</returns>
    public async Task<PagedResultDto<CustomerCategoryDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<customerCategories> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<CustomerCategory>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of customerCategories entities
        var customerCategories = await AsyncExecuter.ToListAsync(queryable);

        // Map the customerCategories entities to customerCategoriesDto objects
        var customerCategoriesDto = ObjectMapper.Map<List<CustomerCategory>, List<CustomerCategoryDto>>(customerCategories);
        // Return the paginated result
        return new PagedResultDto<CustomerCategoryDto>(
            totalCount,
            customerCategoriesDto
        );
    }
}
