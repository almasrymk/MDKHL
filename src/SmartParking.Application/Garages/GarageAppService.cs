using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Garages;

public class GarageAppService : CrudAppService<Garage, GarageDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageDto>, IGarageAppService
{
    private readonly IRepository<Garage> _repository;
    private readonly OrganizationsAppService _organizationsAppService;

    public GarageAppService(IRepository<Garage, int> repository, OrganizationsAppService organizationsAppService) : base(repository)
    {
        _repository = repository;
        _organizationsAppService = organizationsAppService;
    }

    public IRepository<Garage, int> Repository { get; }

    /// <summary>
    /// Method that used to create new garage to specific organization
    /// </summary>
    /// <param name="input">input is createupdateGarageDto type
    /// holds data from presentation layer
    /// </param>
    /// <returns>Dto of created garage</returns>
    /// <exception cref="EntityNotFoundException"> exception is returned when garage id isnot exists </exception>
    public override async Task<GarageDto> CreateAsync(CreateUpdateGarageDto input)
    {
        var organization = await _organizationsAppService.GetAsync(input.OrganizationId);
        if (organization == null)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        string code = organization.Code;
        var numberOfGaragesForOrganization = _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId).Result.Count;
        // Checking criteria for package that organization subscribe.
        input.Code = code + GetOrganizationCode(numberOfGaragesForOrganization);
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
    /// <summary>
    /// methods that deletes camera.
    /// it takes camera id as parameter , checks if it exists
    /// if don't it throws an exception
    /// if exists it deletes it , changes its status to deleted
    /// </summary>
    /// <param name="id">camera id</param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException">exception thrown when camera isn't exist</exception>
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

    /// <summary>
    /// method used to get list of organizations
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not</param>
    /// <returns>OrganizatioDto List</returns>
    public async Task<PagedResultDto<GarageDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<garage> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.Name.StartsWith(input.SearchingTerm) ||
                or.Code.StartsWith(input.SearchingTerm) 
            );
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of garage entities
        var garages = await AsyncExecuter.ToListAsync(queryable);

        // Map the garage entities to garageDto objects
        var garageDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
        Console.WriteLine(garageDto.Count);
        // Return the paginated result
        return new PagedResultDto<GarageDto>(
            totalCount,
            garageDto
        );
    }

    /// <summary>
    /// method used to get list of garages
    /// </summary>
    /// <param name="input">specifies max number of garage and if it filtered or not</param>
    /// <returns>garageDto List</returns>
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
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of garages entities
        var garages = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garageDto objects
        var garageDto = ObjectMapper.Map<List<Garage>, List<GarageDto>>(garages);
        // Return the paginated result
        return new PagedResultDto<GarageDto>(
            totalCount,
            garageDto
        );
    }
}
