using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace SmartParking.Garages;

public class GarageAppService : CrudAppService<Garage, GarageDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageDto>, IGarageAppService
{
    private readonly IRepository<Garage> _repository;
    private readonly OrganizationsAppService _organizationsAppService;
    private IDataFilter _dataFilter;
    private ICurrentUser _currentUser;

    public GarageAppService(IRepository<Garage, int> repository, OrganizationsAppService organizationsAppService , IDataFilter datafilter,ICurrentUser currentUser) : base(repository)
    {
        _repository = repository;
        _organizationsAppService = organizationsAppService;
        _dataFilter = datafilter;
        _currentUser = currentUser;
    }


    

   /// <inheritdoc/>

    public override async Task<GarageDto> CreateAsync(CreateUpdateGarageDto input)
    {
        var organization = await _organizationsAppService.GetAsync(input.OrganizationId);
        if (organization == null)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        string code = organization.Code;
        var numberOfGaragesForOrganization = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
        // Checking criteria for package that organization subscribe.
        input.Code = code + GetOrganizationCode(numberOfGaragesForOrganization.Count);
        return await base.CreateAsync(input);
    }
    /// <summary>
    /// Method is used to create a code for garage
    /// </summary>
    /// <param name="input">input is garage id which is related to specific organization </param>
    /// <returns></returns>
    /// <exception cref="BusinessException">it throws exception when organization tries to add more than 99 garage</exception>

    public string GetOrganizationCode(int input)
    {
        if (input == 99)
        {
            throw new BusinessException("Can't add more garages for that organization");
        }
        string Code = "-G-";
        if (input < 9)
        {
            Code += '0';
        }
        Code += (input + 1);
        return Code;
    }
    /// <inheritdoc/>
    public new async Task<string> DeleteByIdAsync(int id)
    {
        var garage = await base.GetAsync(id);

        if (garage is null)
            throw new EntityNotFoundException(typeof(Garage), id);
        else
        {
            await base.DeleteByIdAsync(id);
            garage.Status = Enums.Status.Deleted;

            return EnumValues.GetEnumDescription(GarageValidations.GarageDeleteion);
        }
    }

    /// <inheritdoc/>
    public async override Task<GarageDto> UpdateAsync(int id, CreateUpdateGarageDto input)
    {
        var organization = await _organizationsAppService.GetAsync(input.OrganizationId);
        if (organization == null)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        return await base.UpdateAsync(id, input);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.Code.StartsWith(input.SearchingTerm) ||
                or.Name.StartsWith(input.SearchingTerm) 
               
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
        var garages = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
        Console.WriteLine(garagesDto.Count);
        // Return the paginated result
        return new PagedResultDto<GarageDto>(
            totalCount,
            garagesDto
        );
    }


    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Garage>.BuildFilterExpression(input.filters);
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
        var garages = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
        Console.WriteLine(garagesDto.Count);
        // Return the paginated result
        return new PagedResultDto<GarageDto>(
            totalCount,
            garagesDto
        );
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<GarageDto>> GetGarageByOrganization(PagedAndSortedResultRequestDto input ,int organizationId)
    {

            var query = await _repository.GetQueryableAsync();
             query=  query.Where(x => x.OrganizationId == organizationId);
            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            query.OrderBy(x => x.Code);
            var garages = await AsyncExecuter.ToListAsync(query);
            var garagesDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
            return new PagedResultDto<GarageDto>(
               totalCount,
               garagesDto
           );
        
    }

    /// <inheritdoc/>
    public async override Task<PagedResultDto<GarageDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (_currentUser.IsInRole("admin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }
        else
            return await base.GetListAsync(input);
    }
}
