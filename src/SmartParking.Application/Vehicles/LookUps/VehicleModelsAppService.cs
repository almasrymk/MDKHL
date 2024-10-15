using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Subscriptions;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.VehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
namespace SmartParking.Vehicles.LookUps;

public class VehicleModelsAppService : CrudAppService<VehicleModel, VehicleModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleModelDto>, IVehicleModelsAppService
{
    private readonly IRepository<VehicleModel, int> _repository;
    private IDataFilter _dataFilter;

    public VehicleModelsAppService(IRepository<VehicleModel, int> repository, IDataFilter dataFilter) : base(repository)
	{
        _repository = repository;
        _dataFilter = dataFilter;
    }
    /// <inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var vehicleModel = await _repository.GetAsync(id);

        if (vehicleModel is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }

        if (CurrentUser.IsInRole("MdkhlAdmin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                await base.DeleteAsync(id);
            }
        }
        else
            await base.DeleteAsync(id);
    }
    public async Task<PagedResultDto<VehicleModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<camera> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.StartsWith(input.SearchingTerm) ||
                or.EnglishName.StartsWith(input.SearchingTerm) 
            );
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of camera entities
        var vehicleModels = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to cameraDto objects
        var vehicleModelsDto = ObjectMapper.Map<List<VehicleModel>, List<VehicleModelDto>>(vehicleModels);
        // Return the paginated result
        return new PagedResultDto<VehicleModelDto>(
            totalCount,
            vehicleModelsDto
        );
    }

    /// <summary>
    /// method used to get list of cameraes
    /// </summary>
    /// <param name="input">specifies max number of camera and if it filtered or not</param>
    /// <returns>cameraeDto List</returns>
    public async Task<PagedResultDto<VehicleModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<camera> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<LicenceType>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of organizations entities
        var vehicleModels = await AsyncExecuter.ToListAsync(queryable);

        // Map the organizations entities to organizationsDto objects
        var vehicleModelsDto = ObjectMapper.Map<List<VehicleModel>, List<VehicleModelDto>>(vehicleModels);
        // Return the paginated result
        return new PagedResultDto<VehicleModelDto>(
            totalCount,
            vehicleModelsDto
        );
    }
}
