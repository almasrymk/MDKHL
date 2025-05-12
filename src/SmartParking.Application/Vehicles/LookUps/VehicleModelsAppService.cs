using Microsoft.AspNetCore.Authorization;
using SmartParking.AlertMessages;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Subscriptions;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Results;
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

[Authorize]
public class VehicleModelsAppService : CrudAppService<VehicleModel, VehicleModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleModelDto>, IVehicleModelsAppService
{
    private readonly IRepository<VehicleModel, int> _repository;
    private readonly IRepository<VehicleBrand, int> _vehicleBrandRepository;
    private IDataFilter _dataFilter;

    public VehicleModelsAppService(IRepository<VehicleModel, int> repository, IRepository<VehicleBrand, int> vehicleBrandRepository, IDataFilter dataFilter)
        : base(repository)
    {
        _repository = repository;
        _vehicleBrandRepository = vehicleBrandRepository;
        _dataFilter = dataFilter;
    }

    #region Public Methods

    ///<inheritdoc/>
    public override async Task<VehicleModelDto> CreateAsync(CreateUpdateVehicleModelDto input)
    {
        await ValidateVehicleBrandExistsAsync(input.VehiclesBrandId);
        await ValidateVehicleModelNameUniquenessAsync(input);
        return await base.CreateAsync(input);
    }

    ///<inheritdoc/>
    public override async Task<VehicleModelDto> UpdateAsync(int id, CreateUpdateVehicleModelDto input)
    {
        var existingVehicleModel = await _repository.GetAsync(id);
        if (existingVehicleModel is null)
        {
            throw new InvalidOperationException("Vehicle model not found");
        }

        await ValidateVehicleBrandExistsAsync(input.VehiclesBrandId);
        await ValidateVehicleModelNameUniquenessAsync(input, id);
        return await base.UpdateAsync(id, input);
    }

    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var vehicleModel = await _repository.FindAsync(id);

        if (vehicleModel is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }

        await _repository.HardDeleteAsync(vehicleModel);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<VehicleModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.VehiclesBrand.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.VehiclesBrand.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        var vehicleModels = await AsyncExecuter.ToListAsync(queryable);
        var vehicleModelsDto = ObjectMapper.Map<List<VehicleModel>, List<VehicleModelDto>>(vehicleModels);

        return new PagedResultDto<VehicleModelDto>(totalCount, vehicleModelsDto);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<VehicleModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        var filterExpression = BuildExpression<VehicleModel>.BuildFilterExpression(input.filters);

        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        var vehicleModels = await AsyncExecuter.ToListAsync(queryable);
        var vehicleModelsDto = ObjectMapper.Map<List<VehicleModel>, List<VehicleModelDto>>(vehicleModels);

        return new PagedResultDto<VehicleModelDto>(totalCount, vehicleModelsDto);
    }

    public async Task<Result<List<VehicleModelDto>>> GetModelsByBrandAsync(int brandId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(m => m.VehiclesBrandId == brandId);
            var models = await AsyncExecuter.ToListAsync(query);
            if (models.Count == 0)
                return Result<List<VehicleModelDto>>.NotFound();

            var modelsMap = ObjectMapper.Map<List<VehicleModel>, List<VehicleModelDto>>(models);
            return Result<List<VehicleModelDto>>.Success(modelsMap);
        }
    }

    public async Task<List<VehicleModelDto>> GetAllVehicleModelsAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var models = await _repository.GetListAsync();
            return ObjectMapper.Map<List<VehicleModel>, List<VehicleModelDto>>(models);
        }
    }

    #endregion

    #region Private Methods

    private async Task ValidateVehicleBrandExistsAsync(int vehicleBrandId)
    {
        var brand = await _vehicleBrandRepository.FindAsync(vehicleBrandId);
        if (brand is null)
        {
            throw new InvalidOperationException("Vehicle brand does not exist");
        }
    }

    private async Task ValidateVehicleModelNameUniquenessAsync(CreateUpdateVehicleModelDto input, int? currentId = null)
    {
        var vehicleModel = await _repository.FirstOrDefaultAsync(vehicle =>
            vehicle.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
            vehicle.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));

        if (vehicleModel != null)
        {
            if (vehicleModel.EnglishName == input.EnglishName.Trim() && vehicleModel.Id != currentId)
            {
                throw new InvalidOperationException("Vehicle model English name already exists");
            }
            else if (vehicleModel.ArabicName == input.ArabicName.Trim() && vehicleModel.Id != currentId)
            {
                throw new InvalidOperationException("Vehicle model Arabic name already exists");
            }
        }
    }

    #endregion
}
