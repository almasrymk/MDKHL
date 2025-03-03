using AutoMapper.Internal.Mappers;
using SmartParking.Entities.Lookups;
using SmartParking.Filters;
using SmartParking.Lookups.CameraModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.LookupsAppServices;

public class CameraModelAppService : CrudAppService<CameraModel, CameraModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateCameraModelDto>, ICameraModelAppService
{
    private readonly IRepository<CameraModel, int> _repository;
    private readonly IDataFilter _dataFilter;

    public CameraModelAppService(IRepository<CameraModel, int> repository, IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }
    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var cameraModel = await _repository.FindAsync(id);

        if (cameraModel is null)
        {
            throw new EntityNotFoundException(typeof(CameraModel), id);
        }

        await _repository.HardDeleteAsync(cameraModel);
    }

    ///<inheritdoc/>
    public override async Task<CameraModelDto> CreateAsync(CreateUpdateCameraModelDto input)
    {
        var cameraModel = await _repository.FirstOrDefaultAsync
           (camera =>
           camera.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
           camera.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


        if (cameraModel != null)
        {
            if (cameraModel.EnglishName == input.EnglishName.Trim(' '))
            {
                throw new InvalidOperationException("Camera model english name already exists");

            }
            else if (cameraModel.ArabicName == input.ArabicName.Trim(' '))
            {
                throw new InvalidOperationException("Camera model  arabic name already exists");

            }


        }
        return await base.CreateAsync(input);

    }

    ///<inheritdoc/>
    public override async Task<CameraModelDto> UpdateAsync(int id, CreateUpdateCameraModelDto input)
    {
        var existingCameraModel = await _repository.GetAsync(id);

        if (existingCameraModel is null)
        {
            throw new InvalidOperationException("Camera model not found");
        }

        var cameraModel = await _repository.FirstOrDefaultAsync
                (camera =>
                camera.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                camera.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


        if (cameraModel != null)
        {
            if (cameraModel.EnglishName == input.EnglishName.Trim(' ') && cameraModel.Id != id)
            {
                throw new InvalidOperationException("Camera model english name already exists");

            }
            else if (cameraModel.ArabicName == input.ArabicName.Trim(' ') && cameraModel.Id != id)
            {
                throw new InvalidOperationException("Camera model arabic name already exists");

            }


        }
        return await base.UpdateAsync(id, input);
    }
    public async Task<PagedResultDto<CameraModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<CameraModel>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of organizations entities
        var cameraModels = await AsyncExecuter.ToListAsync(queryable);

        var cameraModelsDto = ObjectMapper.Map<List<CameraModel>, List<CameraModelDto>>(cameraModels);
        // Return the paginated result
        return new PagedResultDto<CameraModelDto>(
            totalCount,
            cameraModelsDto
        );
    }

    public async Task<List<CameraModelDto>> GetAllCameraModelsAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var models = await _repository.GetListAsync();
            return ObjectMapper.Map<List<CameraModel>, List<CameraModelDto>>(models);
        }
    }

    public async Task<PagedResultDto<CameraModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())


            );
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var cameraModels = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to cameraDto objects
        var cameraModelsDto = ObjectMapper.Map<List<CameraModel>, List<CameraModelDto>>(cameraModels);
        // Return the paginated result
        return new PagedResultDto<CameraModelDto>(
            totalCount,
            cameraModelsDto
        );
    }
}
